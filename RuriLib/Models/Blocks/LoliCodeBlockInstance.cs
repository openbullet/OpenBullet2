using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Configs;
using RuriLib.Models.Proxies;

namespace RuriLib.Models.Blocks;

/// <summary>
/// A block instance that stores raw LoliCode and transpiles its special statements.
/// </summary>
public class LoliCodeBlockInstance : BlockInstance
{
    private const string ValidTokenRegex = "[A-Za-z][A-Za-z0-9_]*";
    private static readonly string NewLine = global::System.Environment.NewLine;

    /// <summary>
    /// Gets or sets the raw LoliCode script.
    /// </summary>
    public string Script { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source line number of the first line in <see cref="Script"/>.
    /// </summary>
    public int SourceLineNumber { get; set; } = 1;

    /// <summary>
    /// Initializes a new <see cref="LoliCodeBlockInstance"/>.
    /// </summary>
    /// <param name="descriptor">The block descriptor.</param>
    public LoliCodeBlockInstance(LoliCodeBlockDescriptor descriptor)
        : base(descriptor)
    {
    }

    /// <inheritdoc />
    public override string ToLC(bool printDefaultParams = false) => Script;

    /// <inheritdoc />
    public override void FromLC(ref string script, ref int lineNumber)
    {
        ArgumentNullException.ThrowIfNull(script);

        Script = script;
        lineNumber += script.CountLines();
    }

    /// <inheritdoc />
    public override IEnumerable<StatementSyntax> ToSyntax(BlockSyntaxGenerationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return StatementSyntaxParser.ParseStatements(BuildScriptSnippet(
            context.DefinedVariables,
            context.StepByStep));
    }

    internal string BuildScriptSnippet(List<string> definedVariables, bool stepByStep = false)
    {
        ArgumentNullException.ThrowIfNull(definedVariables);

        using var reader = new StringReader(Script);
        using var writer = new StringWriter();
        var localLineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            localLineNumber++;
            var trimmedLine = line.Trim();
            var leadingWhitespaceLength = line.Length - line.TrimStart().Length;

            try
            {
                // Try to read it as a LoliCode-exclusive statement
                writer.WriteLine(TranspileStatement(trimmedLine, definedVariables, stepByStep));
            }
            catch (LineParsingException ex)
            {
                throw new LoliCodeParsingException(
                    SourceLineNumber + localLineNumber - 1,
                    leadingWhitespaceLength + (ex.ColumnNumber ?? 1),
                    $"Could not parse LoliCode statement: {trimmedLine.TruncatePretty(50)} ({ex.Message})",
                    ex);
            }
            catch (NotSupportedException)
            {
                // If it failed, we assume what is written is bare C# so we just copy it over (untrimmed)
                writer.WriteLine(line);
            }
        }

        return writer.ToString();
    }

    private string TranspileStatement(string input, List<string> definedVariables, bool stepByStep)
    {
        Match match;

        // (RESOURCES) TAKEONE
        // TAKEONE FROM "MyResource" => "myString"
        if ((match = Regex.Match(input, "TAKEONE FROM (\"[^\"]+\") => @?\"?([^\"]+)\"?")).Success)
        {
            if (definedVariables.Contains(match.Groups[2].Value))
            {
                return AppendDebuggerTracking(
                    $"{match.Groups[2].Value} = globals.Resources[{match.Groups[1].Value}].TakeOne();",
                    match.Groups[2].Value,
                    stepByStep);
            }

            definedVariables.Add(match.Groups[2].Value);
            return AppendDebuggerTracking(
                $"string {match.Groups[2].Value} = globals.Resources[{match.Groups[1].Value}].TakeOne();",
                match.Groups[2].Value,
                stepByStep);
        }

        // (RESOURCES) TAKE
        // TAKE 5 FROM "MyResource" => "myList"
        if ((match = Regex.Match(input, "TAKE ([0-9]+) FROM (\"[^\"]+\") => @?\"?([^\"]+)\"?")).Success)
        {
            if (definedVariables.Contains(match.Groups[3].Value))
            {
                return AppendDebuggerTracking(
                    $"{match.Groups[3].Value} = globals.Resources[{match.Groups[2].Value}].Take({match.Groups[1].Value});",
                    match.Groups[3].Value,
                    stepByStep);
            }

            definedVariables.Add(match.Groups[3].Value);
            return AppendDebuggerTracking(
                $"List<string> {match.Groups[3].Value} = globals.Resources[{match.Groups[2].Value}].Take({match.Groups[1].Value});",
                match.Groups[3].Value,
                stepByStep);
        }

        // CODE LABEL
        // #MYLABEL => MYLABEL:
        if ((match = Regex.Match(input, $"^#({ValidTokenRegex})$")).Success)
        {
            return $"{match.Groups[1].Value}:";
        }

        // JUMP
        // JUMP #MYLABEL => goto MYLABEL;
        if ((match = Regex.Match(input, $"^JUMP #({ValidTokenRegex})$")).Success)
        {
            return $"goto {match.Groups[1].Value};";
        }

        // END
        // END => }
        if (input == "END")
        {
            return "}";
        }

        // REPEAT
        // REPEAT 10 => for (int xyz = 0; xyz < 10; xyz++) {
        if ((match = Regex.Match(input, "^REPEAT (.+)$")).Success)
        {
            var i = VariableNames.RandomName();
            return $"for (var {i} = 0L; {i} < ({match.Groups[1].Value}).AsLong(); {i}++){NewLine}{{";
        }

        // FOREACH
        // FOREACH v IN list => foreach (var v in list) {
        if ((match = Regex.Match(input, $"^FOREACH ({ValidTokenRegex}) IN ({ValidTokenRegex})$")).Success)
        {
            return $"foreach (var {match.Groups[1].Value} in {match.Groups[2].Value}){NewLine}{{";
        }

        // LOG
        // LOG myVar => data.Logger.Log(myVar);
        if ((match = Regex.Match(input, "^LOG (.+)$")).Success)
        {
            return $"data.Logger.LogObject({ConvertStatementArgument(match.Groups[1].Value)});";
        }

        // CLOG
        // CLOG Tomato "hello" => data.Logger.Log("hello", LogColors.Tomato);
        if ((match = Regex.Match(input, "^CLOG ([A-Za-z]+) (.+)$")).Success)
        {
            return $"data.Logger.LogObject({ConvertStatementArgument(match.Groups[2].Value)}, LogColors.{match.Groups[1].Value});";
        }

        // WHILE
        // WHILE a < b => while (a < b) {
        if ((match = Regex.Match(input, "^WHILE (.+)$")).Success)
        {
            var line = match.Groups[1].Value.Trim();
            if (LoliCodeParser.keyIdentifiers.Any(t => line.StartsWith(t, StringComparison.Ordinal)))
            {
                return $"while ({ConvertKeyCondition(input, line)}){NewLine}{{";
            }

            return $"while ({line}){NewLine}{{";
        }

        // IF
        // IF a < b => if (a < b) {
        if ((match = Regex.Match(input, "^IF (.+)$")).Success)
        {
            var line = match.Groups[1].Value.Trim();
            if (LoliCodeParser.keyIdentifiers.Any(t => line.StartsWith(t, StringComparison.Ordinal)))
            {
                return $"if ({ConvertKeyCondition(input, line)}){NewLine}{{";
            }

            return $"if ({line}){NewLine}{{";
        }

        // ELSE
        // ELSE => } else {
        if (input == "ELSE")
        {
            return $"}}{NewLine}else{NewLine}{{";
        }

        // ELSE IF
        // ELSE IF a < b => } else if (a < b) {
        if ((match = Regex.Match(input, "ELSE IF (.+)$")).Success)
        {
            var line = match.Groups[1].Value.Trim();
            if (LoliCodeParser.keyIdentifiers.Any(t => line.StartsWith(t, StringComparison.Ordinal)))
            {
                return $"}}{NewLine}else if ({ConvertKeyCondition(input, line)}){NewLine}{{";
            }

            return $"}}{NewLine}else if ({line}){NewLine}{{";
        }

        // TRY
        // TRY => try {
        if (input == "TRY")
        {
            return $"try{NewLine}{{";
        }

        // CATCH
        // CATCH => } catch {
        if (input == "CATCH")
        {
            return $"}}{NewLine}catch{NewLine}{{";
        }

        // FINALLY
        // FINALLY => } finally {
        if (input == "FINALLY")
        {
            return $"}}{NewLine}finally{NewLine}{{";
        }

        // LOCK
        // LOCK globals => lock (globals) {
        if ((match = Regex.Match(input, "^LOCK (.+)$")).Success)
        {
            return $"lock({match.Groups[1].Value}){NewLine}{{";
        }

        // ACQUIRELOCK
        // ACQUIRELOCK globals => await data.AsyncLocker.Acquire(nameof(globals), data.CancellationToken);
        if ((match = Regex.Match(input, "^ACQUIRELOCK (.+)$")).Success)
        {
            return $"await data.AsyncLocker.Acquire(nameof({match.Groups[1].Value}), data.CancellationToken);";
        }

        // RELEASELOCK
        // RELEASELOCK globals => data.AsyncLocker.Release(nameof(globals));
        if ((match = Regex.Match(input, "^RELEASELOCK (.+)$")).Success)
        {
            return $"data.AsyncLocker.Release(nameof({match.Groups[1].Value}));";
        }

        // SET VAR
        // SET VAR myString "hello" => string myString = "hello";
        if ((match = Regex.Match(input, $"^SET VAR @?\"?({ValidTokenRegex})\"? (.+)$")).Success)
        {
            var value = ConvertStatementArgument(match.Groups[2].Value);

            if (definedVariables.Contains(match.Groups[1].Value))
            {
                return AppendDebuggerTracking(
                    $"{match.Groups[1].Value} = {value};",
                    match.Groups[1].Value,
                    stepByStep);
            }

            definedVariables.Add(match.Groups[1].Value);
            return AppendDebuggerTracking(
                $"string {match.Groups[1].Value} = {value};",
                match.Groups[1].Value,
                stepByStep);
        }

        // SET CAP
        // SET CAP myCapture "hello" => string myString = "hello"; data.MarkForCapture(nameof(myCapture));
        if ((match = Regex.Match(input, $"^SET CAP @?\"?({ValidTokenRegex})\"? (.+)$")).Success)
        {
            var value = ConvertStatementArgument(match.Groups[2].Value);

            if (definedVariables.Contains(match.Groups[1].Value))
            {
                return AppendDebuggerTracking(
                    $"{match.Groups[1].Value} = {value};{NewLine}data.MarkForCapture(nameof({match.Groups[1].Value}));",
                    match.Groups[1].Value,
                    stepByStep);
            }

            definedVariables.Add(match.Groups[1].Value);
            return AppendDebuggerTracking(
                $"string {match.Groups[1].Value} = {value};{NewLine}data.MarkForCapture(nameof({match.Groups[1].Value}));",
                match.Groups[1].Value,
                stepByStep);
        }

        // SET USEPROXY
        // SET USEPROXY TRUE => data.UseProxy = "true";
        if ((match = Regex.Match(input, "^SET USEPROXY (TRUE|FALSE)$")).Success)
        {
            return $"data.UseProxy = {match.Groups[1].Value.ToLowerInvariant()};";
        }

        // SET PROXY
        // SET PROXY "127.0.0.1" 9050 SOCKS5 => data.Proxy = new Proxy("127.0.0.1", 9050, ProxyType.Socks5);
        // SET PROXY "127.0.0.1" 9050 SOCKS5 "username" "password" => data.Proxy = new Proxy("127.0.0.1", 9050, ProxyType.Socks5, "username", "password");
        if (input.StartsWith("SET PROXY ", StringComparison.Ordinal))
        {
            var setProxyParams = input["SET PROXY ".Length..].Split(' ');
            var proxyType = Enum.Parse<ProxyType>(setProxyParams[2], true);

            if (setProxyParams.Length == 3)
            {
                return $"data.Proxy = new Proxy({setProxyParams[0]}, {setProxyParams[1]}, ProxyType.{proxyType});";
            }

            return $"data.Proxy = new Proxy({setProxyParams[0]}, {setProxyParams[1]}, ProxyType.{proxyType}, {setProxyParams[3]}, {setProxyParams[4]});";
        }

        // MARK
        // MARK @myVar => data.MarkForCapture(nameof(myVar));
        if ((match = Regex.Match(input, $"^MARK @?({ValidTokenRegex})$")).Success)
        {
            return $"data.MarkForCapture(nameof({match.Groups[1].Value}));";
        }

        // UNMARK
        // UNMARK @myVar => data.MarkedForCapture.Remove(nameof(myVar));
        if ((match = Regex.Match(input, $"^UNMARK @?({ValidTokenRegex})$")).Success)
        {
            return $"data.UnmarkCapture(nameof({match.Groups[1].Value}));";
        }

        throw new NotSupportedException();
    }

    private static string AppendDebuggerTracking(string statement, string variableName, bool stepByStep)
        => stepByStep && IsDebuggerTrackableVariable(variableName)
            ? $"{statement}{NewLine}data.SetDebuggerVariable(nameof({variableName}), {variableName});"
            : statement;

    private static string ConvertKeyCondition(string statement, string condition)
    {
        var keyLine = condition;
        var keyType = LineParser.ParseToken(ref keyLine);
        var keyArguments = keyLine;

        try
        {
            var key = LoliCodeParser.ParseKey(ref keyArguments, keyType);
            return CSharpWriter.ConvertKey(key);
        }
        catch (LineParsingException ex)
        {
            var conditionColumn = statement.IndexOf(condition, StringComparison.Ordinal) + 1;
            var consumedKeyArgumentColumns = keyLine.Length - keyArguments.Length;

            throw new LineParsingException(
                conditionColumn + keyType.Length + 1 + consumedKeyArgumentColumns + (ex.ColumnNumber ?? 1) - 1,
                ex.Message,
                ex);
        }
    }

    private static string ConvertStatementArgument(string argument)
    {
        var trimmed = argument.TrimStart();

        if (!trimmed.StartsWith('$'))
        {
            return argument;
        }

        var setting = new BlockSetting();
        BlockParameter? parameter = trimmed[1..] switch
        {
            ['"', ..] => new StringParameter(string.Empty),
            ['[', ..] => new ListOfStringsParameter(string.Empty),
            ['{', ..] => new DictionaryOfStringsParameter(string.Empty),
            _ => null
        };

        if (parameter is null)
        {
            return argument;
        }

        LoliCodeParser.ParseSettingValue(ref trimmed, setting, parameter);

        if (!string.IsNullOrWhiteSpace(trimmed))
        {
            throw new LineParsingException(argument.Length - trimmed.Length + 1,
                $"Unexpected text after interpolated literal: {trimmed.TrimStart()}");
        }

        return CSharpWriter.FromSetting(setting);
    }

    private static bool IsDebuggerTrackableVariable(string variableName)
        => !string.IsNullOrWhiteSpace(variableName)
            && !variableName.Contains('.')
            && !variableName.StartsWith("globals.", StringComparison.Ordinal);
}
