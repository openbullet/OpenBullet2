using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Configs;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static RuriLib.Helpers.CSharp.SyntaxDsl;

namespace RuriLib.Models.Blocks;

/// <summary>
/// An instance of a block that was auto generated from exposed methods.
/// </summary>
public class AutoBlockInstance : BlockInstance
{
    private string outputVariable = "output";

    /// <summary>
    /// Gets or sets the output variable written by the block.
    /// </summary>
    public string OutputVariable
    {
        get => outputVariable;
        set => outputVariable = VariableNames.MakeValid(value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the output should be captured.
    /// </summary>
    public bool IsCapture { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the block should run in safe mode.
    /// </summary>
    public bool Safe { get; set; }

    /// <summary>
    /// Initializes an auto-generated block instance.
    /// </summary>
    /// <param name="descriptor">The descriptor that defines the block.</param>
    public AutoBlockInstance(AutoBlockDescriptor descriptor)
        : base(descriptor)
    {
        OutputVariable = descriptor.Id[..1].ToLower() + descriptor.Id[1..] + "Output";
    }

    /// <inheritdoc />
    public override string ToLC(bool printDefaultParams = false)
    {
        /*
         *   SettingName = "my value"
         *   SettingName = 0
         *   SettingName = @myVariable
         */

        using var writer = new LoliCodeWriter(base.ToLC(printDefaultParams));

        if (Safe)
        {
            writer.AppendLine("SAFE", 2);
        }

        var outVarKind = IsCapture ? "CAP" : "VAR";

        // Write the output variable
        if (Descriptor.ReturnType.HasValue)
        {
            writer.AppendLine($"=> {outVarKind} @{OutputVariable}", 2);
        }

        return writer.ToString();
    }

    /// <inheritdoc />
    public override void FromLC(ref string script, ref int lineNumber)
    {
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

            if (line.StartsWith("SAFE"))
            {
                Safe = true;
            }
            else if (line.StartsWith("=>"))
            {
                try
                {
                    var match = Regex.Match(line, "^=> ([A-Za-z]{3}) (.*)$");
                    IsCapture = match.Groups[1].Value.Equals("CAP", StringComparison.OrdinalIgnoreCase);
                    OutputVariable = match.Groups[2].Value.Trim()[1..];
                }
                catch
                {
                    throw new LoliCodeParsingException(lineNumber,
                        $"The output variable declaration is in the wrong format: {lineCopy.TruncatePretty(50)}");
                }
            }
            else
            {
                try
                {
                    LoliCodeParser.ParseSetting(ref line, Settings, Descriptor);
                }
                catch (LineParsingException ex)
                {
                    throw new LoliCodeParsingException(lineNumber, ex.ColumnNumber ?? 1,
                        $"Could not parse the setting: {lineCopy.TruncatePretty(50)} ({ex.Message})", ex);
                }
                catch (Exception ex)
                {
                    throw new LoliCodeParsingException(lineNumber,
                        $"Could not parse the setting: {lineCopy.TruncatePretty(50)} ({ex.Message})");
                }
            }
        }
    }

    /// <inheritdoc />
    public override IEnumerable<StatementSyntax> ToSyntax(BlockSyntaxGenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var statements = new List<StatementSyntax>();

        if (Safe)
        {
            if (Descriptor.ReturnType.HasValue
                && !context.DefinedVariables.Contains(OutputVariable)
                && !OutputVariable.StartsWith("globals."))
            {
                if (!Disabled)
                {
                    context.DefinedVariables.Add(OutputVariable);
                }

                statements.Add(BlockSyntaxFactory.CreateVariableDeclaration(
                    GetRuntimeReturnType(),
                    OutputVariable,
                    Expr(GetDefaultReturnValue())));
            }

            var tryStatements = CreateExecutionStatements(context.DefinedVariables, true);
            statements.Add(SyntaxFactory.TryStatement(
                SyntaxFactory.Block(tryStatements),
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

    // This is needed otherwise when we have blocks made in other plugins they might reference
    // types from different runtimes and our castings like .AsBool() or .AsInt() will throw a
    // RuntimeBinderException, so we cannot just write 'var' but we need to explicitly write the type.
    private string GetRuntimeReturnType() => Descriptor.ReturnType switch
    {
        VariableType.Bool => "bool",
        VariableType.ByteArray => "byte[]",
        VariableType.DictionaryOfStrings => "Dictionary<string, string>",
        VariableType.Float => "double",
        VariableType.Int => "long",
        VariableType.ListOfStrings => "List<string>",
        VariableType.String => "string",
        _ => throw new NotSupportedException()
    };

    private string GetDefaultReturnValue() => Descriptor.ReturnType switch
    {
        VariableType.Bool => "false",
        VariableType.ByteArray => "Array.Empty<byte>()",
        VariableType.DictionaryOfStrings => "new()",
        VariableType.Float => "0",
        VariableType.Int => "0",
        VariableType.ListOfStrings => "new()",
        VariableType.String => "string.Empty",
        _ => throw new NotSupportedException()
    };

    private List<StatementSyntax> CreateExecutionStatements(
        List<string> declaredVariables,
        bool assignmentOnly,
        bool trackDebuggerVariable = false)
    {
        var statements = new List<StatementSyntax>();

        if (Descriptor.ReturnType.HasValue)
        {
            var invocationExpression = BuildMethodInvocationExpression();
            var assignToExistingVariable = assignmentOnly
                || declaredVariables.Contains(OutputVariable)
                || OutputVariable.StartsWith("globals.");

            if (!assignToExistingVariable && !Disabled)
            {
                declaredVariables.Add(OutputVariable);
            }

            statements.Add(BlockSyntaxFactory.CreateVariableDeclarationOrAssignment(
                GetRuntimeReturnType(),
                OutputVariable,
                invocationExpression,
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
        }
        else
        {
            statements.Add(BuildMethodInvocationExpression().Stmt());
        }

        return statements;
    }

    private bool ShouldTrackDebuggerVariable(BlockSyntaxGenerationContext context)
        => context.StepByStep
            && Descriptor.ReturnType.HasValue
            && !OutputVariable.StartsWith("globals.", StringComparison.Ordinal);

    private ExpressionSyntax BuildMethodInvocationExpression()
    {
        var descriptor = (AutoBlockDescriptor)Descriptor;
        var arguments = new List<ExpressionSyntax> { Id("data") };

        arguments.AddRange(Settings.Values.Select(CSharpWriter.FromSettingSyntax));

        var methodName = string.IsNullOrWhiteSpace(descriptor.MethodName)
            ? descriptor.Id
            : descriptor.MethodName;

        ExpressionSyntax invocation = Id(methodName).Call(arguments);

        return descriptor.Async
            ? BlockSyntaxFactory.CreateAwaitConfigureAwaitFalse(invocation)
            : invocation;
    }
}
