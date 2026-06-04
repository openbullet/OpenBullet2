using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Parse;
using RuriLib.Models.Configs;
using static RuriLib.Helpers.CSharp.SyntaxDsl;

namespace RuriLib.Models.Blocks.Custom;

/// <summary>
/// Block instance for the custom parse block.
/// </summary>
public class ParseBlockInstance(ParseBlockDescriptor descriptor) : BlockInstance(descriptor)
{
    private string outputVariable = "parseOutput";

    /// <summary>
    /// Gets or sets the output variable written by the block.
    /// </summary>
    public string OutputVariable
    {
        get => outputVariable;
        set => outputVariable = VariableNames.MakeValid(value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether multiple results should be returned.
    /// </summary>
    public bool Recursive { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the output should be captured.
    /// </summary>
    public bool IsCapture { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether safe mode is enabled.
    /// </summary>
    public bool Safe { get; set; }
    /// <summary>
    /// Gets or sets the parsing mode.
    /// </summary>
    public ParseMode Mode { get; set; } = ParseMode.LR;

    /// <inheritdoc />
    public override string ToLC(bool printDefaultParams = false)
    {
        /*
         *   recursive = True
         *   mode = LR
         *   input = "hello how are you"
         *   leftDelim = "hello"
         *   rightDelim = "you"
         *   caseSensitive = True
         *   => CAP PARSED
         */

        using var writer = new LoliCodeWriter(base.ToLC(printDefaultParams));

        if (Safe)
        {
            writer.AppendLine("SAFE", 2);
        }

        if (Recursive)
        {
            writer.AppendLine("RECURSIVE", 2);
        }

        writer.AppendLine($"MODE:{Mode}", 2);

        var isCap = IsCapture ? "CAP" : "VAR";
        writer.AppendLine($"=> {isCap} @{OutputVariable}", 2);

        return writer.ToString();
    }

    /// <inheritdoc />
    public override void FromLC(ref string script, ref int lineNumber)
    {
        /*
         *   recursive = True
         *   mode = LR
         *   input = "hello how are you"
         *   leftDelim = "hello"
         *   rightDelim = "you"
         *   caseSensitive = True
         *   => CAP PARSED
         */

        ArgumentNullException.ThrowIfNull(script);

        // First parse the options that are common to every BlockInstance
        base.FromLC(ref script, ref lineNumber);

        using var reader = new StringReader(script);

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            lineNumber++;
            var lineCopy = line;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("SAFE", StringComparison.Ordinal))
            {
                Safe = true;
                continue;
            }

            if (line.StartsWith("RECURSIVE", StringComparison.Ordinal))
            {
                Recursive = true;
            }
            else if (line.StartsWith("MODE", StringComparison.Ordinal))
            {
                try
                {
                    Mode = Enum.Parse<ParseMode>(Regex.Match(line, "MODE:([A-Za-z]+)").Groups[1].Value);
                }
                catch
                {
                    throw new LoliCodeParsingException(lineNumber, $"Could not understand the parsing mode: {lineCopy.TruncatePretty(50)}");
                }
            }
            else if (line.StartsWith("=>", StringComparison.Ordinal))
            {
                try
                {
                    var match = Regex.Match(line, "^=> ([A-Za-z]{3}) @(.+)$");

                    if (!match.Success)
                    {
                        throw new FormatException();
                    }

                    IsCapture = match.Groups[1].Value.Equals("CAP", StringComparison.OrdinalIgnoreCase);
                    OutputVariable = match.Groups[2].Value.Trim();
                }
                catch
                {
                    throw new LoliCodeParsingException(lineNumber, $"The output variable declaration is in the wrong format: {lineCopy.TruncatePretty(50)}");
                }
            }
            else
            {
                try
                {
                    LoliCodeParser.ParseSetting(ref line, Settings, Descriptor);
                }
                catch
                {
                    throw new LoliCodeParsingException(lineNumber, $"Could not parse the setting: {lineCopy.TruncatePretty(50)}");
                }
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<StatementSyntax> ToSyntax(BlockSyntaxGenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var statements = new List<StatementSyntax>();
        var outputType = Recursive ? "List<string>" : "string";
        var defaultReturnValue = Recursive ? "new List<string>()" : "string.Empty";

        if (Safe)
        {
            if (!context.DefinedVariables.Contains(OutputVariable)
                && !OutputVariable.StartsWith("globals.", StringComparison.Ordinal))
            {
                if (!Disabled)
                {
                    context.DefinedVariables.Add(OutputVariable);
                }

                statements.Add(BlockSyntaxFactory.CreateVariableDeclaration(
                    outputType,
                    OutputVariable,
                    Expr(defaultReturnValue)));
            }

            statements.Add(SyntaxFactory.TryStatement(
                SyntaxFactory.Block(CreateExecutionStatements(context.DefinedVariables, true).ToArray()),
                SyntaxFactory.List([BlockSyntaxFactory.CreateSafeModeCatchClause()]),
                null));

            if (ShouldTrackDebuggerVariable(context))
            {
                statements.Add(BlockSyntaxFactory.CreateDataMethodWithNameofAndValueArgument(
                    "SetDebuggerVariable",
                    OutputVariable));
            }

            return statements;
        }

        statements.AddRange(CreateExecutionStatements(context.DefinedVariables, false, context.StepByStep));
        return statements;
    }

    private List<StatementSyntax> CreateExecutionStatements(
        List<string> definedVariables,
        bool assignmentOnly,
        bool trackDebuggerVariable = false)
    {
        var statements = new List<StatementSyntax>();
        var outputType = Recursive ? "List<string>" : "string";
        var invocation = BuildParseInvocationExpression();
        var assignToExistingVariable = assignmentOnly
            || definedVariables.Contains(OutputVariable)
            || OutputVariable.StartsWith("globals.", StringComparison.Ordinal);

        if (!assignToExistingVariable && !Disabled)
        {
            definedVariables.Add(OutputVariable);
        }

        statements.Add(BlockSyntaxFactory.CreateVariableDeclarationOrAssignment(
            outputType,
            OutputVariable,
            invocation,
            assignToExistingVariable));

        statements.Add(BlockSyntaxFactory.CreateDataMethodWithNameofArgument("LogVariableAssignment", OutputVariable));

        if (trackDebuggerVariable && !OutputVariable.StartsWith("globals.", StringComparison.Ordinal))
        {
            statements.Add(BlockSyntaxFactory.CreateDataMethodWithNameofAndValueArgument(
                "SetDebuggerVariable",
                OutputVariable));
        }

        if (IsCapture)
        {
            statements.Add(BlockSyntaxFactory.CreateDataMethodWithNameofArgument("MarkForCapture", OutputVariable));
        }

        return statements;
    }

    private bool ShouldTrackDebuggerVariable(BlockSyntaxGenerationContext context)
        => context.StepByStep && !OutputVariable.StartsWith("globals.", StringComparison.Ordinal);

    private ExpressionSyntax BuildParseInvocationExpression()
    {
        var methodName = Mode switch
        {
            ParseMode.LR => "ParseBetweenStrings",
            ParseMode.CSS => "QueryCssSelector",
            ParseMode.XPath => "QueryXPath",
            ParseMode.Json => "QueryJsonToken",
            ParseMode.Regex => "MatchRegexGroups",
            _ => throw new NotSupportedException()
        };

        if (Recursive)
        {
            methodName += "Recursive";
        }

        var arguments = new List<ExpressionSyntax>
        {
            Id("data"),
            CSharpWriter.FromSettingSyntax(Settings["input"])
        };

        switch (Mode)
        {
            case ParseMode.LR:
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["leftDelim"]));
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["rightDelim"]));
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["caseSensitive"]));
                break;

            case ParseMode.CSS:
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["cssSelector"]));
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["attributeName"]));
                break;

            case ParseMode.XPath:
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["xPath"]));
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["attributeName"]));
                break;

            case ParseMode.Json:
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["jToken"]));
                break;

            case ParseMode.Regex:
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["pattern"]));
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["outputFormat"]));
                arguments.Add(CSharpWriter.FromSettingSyntax(Settings["multiLine"]));
                break;
        }

        arguments.Add(CSharpWriter.FromSettingSyntax(Settings["prefix"]));
        arguments.Add(CSharpWriter.FromSettingSyntax(Settings["suffix"]));
        arguments.Add(CSharpWriter.FromSettingSyntax(Settings["urlEncodeOutput"]));

        return Id(methodName).Call(arguments);
    }
}
