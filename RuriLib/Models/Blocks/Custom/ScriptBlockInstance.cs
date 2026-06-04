using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Functions.Conversion;
using RuriLib.Functions.Crypto;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Script;
using RuriLib.Models.Configs;
using RuriLib.Models.Variables;
using static RuriLib.Helpers.CSharp.SyntaxDsl;

namespace RuriLib.Models.Blocks.Custom;

/// <summary>
/// Block instance for invoking a script in another language.
/// </summary>
public class ScriptBlockInstance : BlockInstance
{
    /// <summary>
    /// Gets or sets the script body.
    /// </summary>
    public string Script { get; set; } = "var result = x + y;";

    /// <summary>
    /// Gets or sets the output variables exposed by the script.
    /// </summary>
    public List<OutputVariable> OutputVariables { get; set; } =
    [
        new()
        {
            Name = "result",
            Type = VariableType.Int
        }
    ];

    /// <summary>
    /// Gets or sets the comma-separated input variable list.
    /// </summary>
    public string InputVariables { get; set; } = "x,y";
    /// <summary>
    /// Gets or sets the interpreter used to run the script.
    /// </summary>
    public Interpreter Interpreter { get; set; } = Interpreter.NodeJS;

    /// <summary>
    /// Initializes a new <see cref="ScriptBlockInstance"/>.
    /// </summary>
    /// <param name="descriptor">The block descriptor.</param>
    public ScriptBlockInstance(ScriptBlockDescriptor descriptor)
        : base(descriptor)
    {
    }

    /// <inheritdoc />
    public override string ToLC(bool printDefaultParams = false)
    {
        /*
         *   INTERPRETER:Jint
         *   INPUT x,y
         *   BEGIN SCRIPT
         *   var result = x + y;
         *   END SCRIPT
         *   OUTPUT Int result
         */

        using var writer = new LoliCodeWriter(base.ToLC(printDefaultParams));
        writer.WriteLine($"INTERPRETER:{Interpreter}");

        writer.WriteLine($"INPUT {InputVariables}");
        writer.WriteLine("BEGIN SCRIPT");
        writer.WriteLine(TrimTrailingLineEndings(Script));
        writer.WriteLine("END SCRIPT");

        foreach (var output in OutputVariables)
        {
            writer.WriteLine($"OUTPUT {output.Type} @{output.Name}");
        }

        return writer.ToString();
    }

    /// <inheritdoc />
    public override void FromLC(ref string script, ref int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(script);

        // First parse the options that are common to every BlockInstance
        base.FromLC(ref script, ref lineNumber);

        using var reader = new StringReader(script);
        using var writer = new StringWriter();

        var interpreterLine = reader.ReadLine();
        lineNumber++;

        if (interpreterLine is null)
        {
            throw new LoliCodeParsingException(lineNumber, "Missing interpreter definition");
        }

        try
        {
            Interpreter = Enum.Parse<Interpreter>(
                Regex.Match(interpreterLine, "INTERPRETER:([^ ]+)$").Groups[1].Value);
        }
        catch
        {
            throw new LoliCodeParsingException(lineNumber,
                $"Invalid interpreter definition: {interpreterLine.TruncatePretty(50)}");
        }

        var inputVariablesLine = reader.ReadLine();
        lineNumber++;

        if (Interpreter == Interpreter.Python && inputVariablesLine?.StartsWith("PYTHONVERSION:", StringComparison.Ordinal) == true)
        {
            inputVariablesLine = reader.ReadLine();
            lineNumber++;
        }

        if (inputVariablesLine is null)
        {
            throw new LoliCodeParsingException(lineNumber, "Missing input variables definition");
        }

        try
        {
            InputVariables = Regex.Match(inputVariablesLine, "INPUT (.*)$").Groups[1].Value;
        }
        catch
        {
            throw new LoliCodeParsingException(lineNumber, "Invalid input variables definition");
        }

        var beginScriptLine = reader.ReadLine();
        lineNumber++;

        if (beginScriptLine != "BEGIN SCRIPT")
        {
            throw new LoliCodeParsingException(lineNumber,
                $"Invalid script start definition: {beginScriptLine?.TruncatePretty(50) ?? "<null>"}");
        }

        var foundEndScript = false;
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;

            if (line == "END SCRIPT")
            {
                foundEndScript = true;
                break;
            }

            writer.WriteLine(line);
        }

        if (!foundEndScript)
        {
            throw new LoliCodeParsingException(lineNumber, "Missing END SCRIPT definition");
        }

        Script = TrimTrailingLineEndings(writer.ToString()); // Remove blank lines at the end

        OutputVariables = [];
        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            var match = Regex.Match(line, "OUTPUT ([^ ]+) @([^ ]+)$");

            try
            {
                OutputVariables.Add(new OutputVariable
                {
                    Type = Enum.Parse<VariableType>(match.Groups[1].Value),
                    Name = match.Groups[2].Value
                });
            }
            catch
            {
                // TODO: Warn the user that the output variable is invalid
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<StatementSyntax> ToSyntax(BlockSyntaxGenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var statements = new List<StatementSyntax>();
        var resultName = "tmp_" + VariableNames.RandomName(6);
        var engineName = "tmp_" + VariableNames.RandomName(6);
        var scopeName = "tmp_" + VariableNames.RandomName(6);

        switch (Interpreter)
        {
            case Interpreter.Jint:
                var scriptPath = EnsureScriptFile(Script, Interpreter);

                statements.Add(BlockSyntaxFactory.CreateVariableDeclaration(
                    "var",
                    engineName,
                    New("Engine")));

                foreach (var input in GetInputs())
                {
                    statements.Add(Id(engineName).Call(
                        "SetValue",
                        NameOf(input),
                        Expr(input)).Stmt());
                }

                statements.Add(Expr(engineName).Assign(
                    Id("InvokeJint").Call(Id("data"), Id(engineName), Lit(scriptPath))));

                foreach (var output in OutputVariables)
                {
                    statements.AddRange(CreateOutputAssignmentStatements(
                        context.DefinedVariables,
                        output,
                        BuildJintOutputExpression(engineName, output),
                        context.StepByStep));
                }

                break;

            case Interpreter.NodeJS:
                var nodeRequireBasePath = JsonConvert.ToString(
                    Path.GetFullPath(Path.Combine("Scripts", "__ob2_virtual__.js")));
                var nodeScript = $$"""
                    module.exports = async ({{MakeInputs()}}) => {
                        const { createRequire } = require('module');
                        const obRequire = createRequire({{nodeRequireBasePath}});
                        return await (async (require) => {
                            {{Script}}
                            var noderesult = {
                            {{MakeNodeObject()}}
                            };
                            return noderesult;
                        })(obRequire);
                    }
                    """;

                var nodeScriptHash = HexConverter.ToHexString(Crypto.MD5(Encoding.UTF8.GetBytes(nodeScript)));
                var escapedScript = JsonConvert.ToString(nodeScript);
                var nodeInvocation = Expr("InvokeNode<dynamic>").Call(
                    Id("data"),
                    Expr(escapedScript),
                    BuildInputArrayExpression(),
                    Lit(true),
                    Lit(nodeScriptHash));

                statements.Add(BlockSyntaxFactory.CreateVariableDeclaration(
                    "var",
                    resultName,
                    nodeInvocation.Await()));

                foreach (var output in OutputVariables)
                {
                    statements.AddRange(CreateOutputAssignmentStatements(
                        context.DefinedVariables,
                        output,
                        BuildNodeOutputExpression(resultName, output),
                        context.StepByStep));
                }

                break;

            case Interpreter.IronPython:
                scriptPath = EnsureScriptFile(Script, Interpreter);

                statements.Add(BlockSyntaxFactory.CreateVariableDeclaration(
                    "var",
                    scopeName,
                    Id("GetIronPyScope").Call(Id("data"))));

                foreach (var input in GetInputs())
                {
                    statements.Add(Id(scopeName).Call(
                        "SetVariable",
                        NameOf(input),
                        Expr(input)).Stmt());
                }

                statements.Add(Id("ExecuteIronPyScript").Call(
                    Id("data"),
                    Id(scopeName),
                    Lit(scriptPath)).Stmt());

                foreach (var output in OutputVariables)
                {
                    statements.AddRange(CreateOutputAssignmentStatements(
                        context.DefinedVariables,
                        output,
                        BuildIronPythonOutputExpression(scopeName, output),
                        context.StepByStep));
                }

                break;

            case Interpreter.Python:
                statements.Add(BlockSyntaxFactory.CreateVariableDeclaration(
                    "var",
                    resultName,
                    Id("InvokePythonAsync").Call(
                        Id("data"),
                        Lit(Script),
                        Lit(GetScriptHash(Script)),
                        BuildSanitizedInputArrayExpression(),
                        BuildInputArrayExpression(),
                        BuildOutputNameArrayExpression(),
                        BuildOutputTypeArrayExpression()).Await()));

                foreach (var output in OutputVariables)
                {
                    statements.AddRange(CreateOutputAssignmentStatements(
                        context.DefinedVariables,
                        output,
                        BuildPythonOutputExpression(resultName, output),
                        context.StepByStep));
                }

                break;
        }

        foreach (var output in OutputVariables)
        {
            statements.Add(BlockSyntaxFactory.CreateDataMethodWithNameofArgument("LogVariableAssignment", output.Name));
        }

        return statements;
    }

    private ExpressionSyntax BuildNodeOutputExpression(string resultName, OutputVariable output)
        => Expr(GetJsonOutputAccessor(resultName, output));

    private string GetJsonOutputAccessor(string resultName, OutputVariable output)
        => output.Type switch
        {
            VariableType.Bool => $"{resultName}.GetProperty(\"{output.Name}\").GetBoolean()",
            VariableType.ByteArray => $"{resultName}.GetProperty(\"{output.Name}\").GetBytesFromBase64()",
            VariableType.Float => $"{resultName}.GetProperty(\"{output.Name}\").GetSingle()",
            VariableType.Int => $"{resultName}.GetProperty(\"{output.Name}\").GetInt32()",
            VariableType.String => $"{resultName}.GetProperty(\"{output.Name}\").ToString()",
            VariableType.ListOfStrings => $"((System.Text.Json.JsonElement.ArrayEnumerator){resultName}.GetProperty(\"{output.Name}\").EnumerateArray()).Select(e => e.GetString()).ToList()",
            VariableType.DictionaryOfStrings => $"((System.Text.Json.JsonElement.ObjectEnumerator){resultName}.GetProperty(\"{output.Name}\").EnumerateObject()).ToDictionary(e => e.Name, e => e.Value.GetString())",
            _ => throw new NotImplementedException()
        };

    private string GetJintMethod(VariableType type)
        => type switch
        {
            VariableType.Bool => "AsBoolean()",
            VariableType.ByteArray => "TryCast<byte[]>()",
            VariableType.Float => "AsNumber().ToSingle()",
            VariableType.Int => "AsNumber().ToInt()",
            VariableType.ListOfStrings => "AsArray().GetEnumerator().ToEnumerable().Select(j => j.ToString()).ToList()",
            VariableType.String => "ToString()",
            _ => throw new NotImplementedException() // Dictionary not implemented yet
        };

    private ExpressionSyntax BuildJintOutputExpression(string engineName, OutputVariable output)
        => Expr(
            $"{engineName}.Global.GetProperty(\"{output.Name}\").Value.{GetJintMethod(output.Type)}");

    private ExpressionSyntax BuildIronPythonOutputExpression(string scopeName, OutputVariable output)
        => Expr(output.Type switch
        {
            VariableType.ListOfStrings => $"{scopeName}.GetVariable<IList<object>>(\"{output.Name}\").Cast<string>().ToList()",
            VariableType.ByteArray => $"{scopeName}.GetVariable<IList<object>>(\"{output.Name}\").Cast<byte>().ToArray()",
            _ => $"{scopeName}.GetVariable<{GetRuntimeTypeName(output.Type)}>(\"{output.Name}\")"
        });

    private ExpressionSyntax BuildPythonOutputExpression(string resultName, OutputVariable output)
        => Expr(GetPythonOutputAccessor(resultName, output));

    private string GetPythonOutputAccessor(string resultName, OutputVariable output)
        => output.Type switch
        {
            VariableType.Bool => $"GetPythonBoolOutput({resultName}, \"{output.Name}\")",
            VariableType.ByteArray => $"GetPythonByteArrayOutput({resultName}, \"{output.Name}\")",
            VariableType.Float => $"GetPythonFloatOutput({resultName}, \"{output.Name}\")",
            VariableType.Int => $"GetPythonIntOutput({resultName}, \"{output.Name}\")",
            VariableType.String => $"GetPythonStringOutput({resultName}, \"{output.Name}\")",
            VariableType.ListOfStrings => $"GetPythonListOfStringsOutput({resultName}, \"{output.Name}\")",
            VariableType.DictionaryOfStrings => $"GetPythonDictionaryOfStringsOutput({resultName}, \"{output.Name}\")",
            _ => throw new NotImplementedException()
        };

    private string GetRuntimeTypeName(VariableType type)
        => type switch
        {
            VariableType.Bool => "bool",
            VariableType.ByteArray => "byte[]",
            VariableType.Float => "float",
            VariableType.Int => "int",
            VariableType.ListOfStrings => "List<string>",
            VariableType.String => "string",
            VariableType.DictionaryOfStrings => "Dictionary<string, string>",
            _ => throw new NotImplementedException()
        };

    private string MakeNodeObject()
        => string.Join(global::System.Environment.NewLine, OutputVariables.Select(o => $"  '{o.Name}': {o.Name},"));

    private static string GetScriptHash(string script)
        => HexConverter.ToHexString(Crypto.MD5(Encoding.UTF8.GetBytes(script)));

    private static string TrimTrailingLineEndings(string value)
        => value.TrimEnd('\r', '\n');

    private string MakeInputs()
        => string.Join(",", GetInputs().Select(SanitizeInput));

    private IEnumerable<string> GetInputs()
        => string.IsNullOrWhiteSpace(InputVariables)
            ? []
            : InputVariables.Split(',')
                .Select(i => i.Trim())
                .Where(i => !string.IsNullOrWhiteSpace(i));

    // Converts input.DATA into DATA
    private string SanitizeInput(string input)
        => Regex.Match(input, "[A-Za-z0-9_]+$").Value;

    private static string EnsureScriptFile(string script, Interpreter interpreter)
    {
        var scriptHash = GetScriptHash(script);
        var scriptPath = $"Scripts/{scriptHash}.{interpreter switch
        {
            Interpreter.Jint => "js",
            Interpreter.NodeJS => "js",
            Interpreter.IronPython => "py",
            _ => throw new NotImplementedException()
        }}";

        if (!Directory.Exists("Scripts"))
        {
            Directory.CreateDirectory("Scripts");
        }

        if (!File.Exists(scriptPath))
        {
            File.WriteAllText(scriptPath, script);
        }

        return scriptPath;
    }

    private ExpressionSyntax BuildInputArrayExpression()
        => ArrayOf("object", GetInputs().Select(Expr).ToArray());

    private ExpressionSyntax BuildSanitizedInputArrayExpression()
        => ArrayOf("string", GetInputs().Select(SanitizeInput).Select(Lit).ToArray());

    private ExpressionSyntax BuildOutputNameArrayExpression()
        => ArrayOf("string", OutputVariables.Select(o => Lit(o.Name)).ToArray());

    private ExpressionSyntax BuildOutputTypeArrayExpression()
        => ArrayOf(
            "global::RuriLib.Models.Variables.VariableType",
            OutputVariables.Select(o => Expr($"global::RuriLib.Models.Variables.VariableType.{o.Type}")).ToArray());

    private IEnumerable<StatementSyntax> CreateOutputAssignmentStatements(
        List<string> definedVariables,
        OutputVariable output,
        ExpressionSyntax expression,
        bool trackDebuggerVariable)
    {
        var assignToExistingVariable = definedVariables.Contains(output.Name);

        if (!assignToExistingVariable)
        {
            definedVariables.Add(output.Name);
        }

        yield return BlockSyntaxFactory.CreateVariableDeclarationOrAssignment(
            GetRuntimeTypeName(output.Type),
            output.Name,
            expression,
            assignToExistingVariable);

        if (trackDebuggerVariable && !output.Name.StartsWith("globals.", StringComparison.Ordinal))
        {
            yield return BlockSyntaxFactory.CreateDataMethodWithNameofAndValueArgument(
                "SetDebuggerVariable",
                output.Name);
        }
    }
}
