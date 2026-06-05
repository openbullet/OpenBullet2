using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Configs;
using static RuriLib.Helpers.CSharp.SyntaxDsl;

namespace RuriLib.Models.Blocks.Custom;

/// <summary>
/// Block instance for the custom keycheck block.
/// </summary>
public class KeycheckBlockInstance(KeycheckBlockDescriptor descriptor) : BlockInstance(descriptor)
{
    /// <summary>
    /// Gets or sets the configured keychains.
    /// </summary>
    public List<Keychain> Keychains { get; set; } = [];

    /// <inheritdoc />
    public override string ToLC(bool printDefaultParams = false)
    {
        /*
         *   KEYCHAIN SUCCESS OR
         *     STRINGKEY @myVariable Contains "abc"
         *     DICTKEY @data.COOKIES HasKey "my-cookie"
         *   KEYCHAIN FAIL AND
         *     LISTKEY @myList Contains "item"
         *     FLOATKEY 1 GreaterThan 2
         */

        using var writer = new LoliCodeWriter(base.ToLC(printDefaultParams));

        // Write all the keychains
        foreach (var keychain in Keychains)
        {
            writer
                .AppendToken("KEYCHAIN", 2)
                .AppendToken(keychain.ResultStatus)
                .AppendLine(keychain.Mode.ToString());

            foreach (var key in keychain.Keys)
            {
                (var keyName, var comparison) = key switch
                {
                    BoolKey x => ("BOOLKEY", x.Comparison.ToString()),
                    StringKey x => ("STRINGKEY", x.Comparison.ToString()),
                    IntKey x => ("INTKEY", x.Comparison.ToString()),
                    FloatKey x => ("FLOATKEY", x.Comparison.ToString()),
                    DictionaryKey x => ("DICTKEY", x.Comparison.ToString()),
                    ListKey x => ("LISTKEY", x.Comparison.ToString()),
                    _ => throw new InvalidOperationException("Unknown key type")
                };

                writer
                    .AppendToken(keyName, 4)
                    .AppendToken(LoliCodeWriter.GetSettingValue(key.Left))
                    .AppendToken(comparison)
                    .AppendLine(LoliCodeWriter.GetSettingValue(key.Right));
            }
        }

        return writer.ToString();
    }

    /// <inheritdoc />
    public override void FromLC(ref string script, ref int lineNumber)
    {
        /*
         *   KEYCHAIN SUCCESS OR
         *     STRINGKEY @myVariable Contains "abc"
         *     DICTKEY @data.COOKIES HasKey "my-cookie"
         *   KEYCHAIN FAIL AND
         *     LISTKEY @myList Contains "item"
         *     FLOATKEY 1 GreaterThan 2
         */

        ArgumentNullException.ThrowIfNull(script);

        // First parse the options that are common to every BlockInstance
        base.FromLC(ref script, ref lineNumber);

        using var reader = new StringReader(script);

        while (reader.ReadLine() is { } line)
        {
            line = line.Trim();
            var lineCopy = line;
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("KEYCHAIN", StringComparison.Ordinal))
            {
                try
                {
                    var keychain = new Keychain();
                    LineParser.ParseToken(ref line);
                    keychain.ResultStatus = LineParser.ParseToken(ref line);
                    keychain.Mode = Enum.Parse<KeychainMode>(LineParser.ParseToken(ref line));
                    Keychains.Add(keychain);
                }
                catch (LineParsingException ex)
                {
                    throw new LoliCodeParsingException(lineNumber, ex.ColumnNumber ?? 1,
                        $"Invalid keychain declaration: {lineCopy.TruncatePretty(50)} ({ex.Message})", ex);
                }
                catch
                {
                    throw new LoliCodeParsingException(lineNumber, $"Invalid keychain declaration: {lineCopy.TruncatePretty(50)}");
                }
            }
            else if (Regex.IsMatch(line, "^[A-Z]+KEY "))
            {
                try
                {
                    if (Keychains.Count == 0)
                    {
                        throw new FormatException();
                    }

                    var keyType = LineParser.ParseToken(ref line);
                    Keychains[^1].Keys.Add(LoliCodeParser.ParseKey(ref line, keyType));
                }
                catch (LineParsingException ex)
                {
                    throw new LoliCodeParsingException(lineNumber, ex.ColumnNumber ?? 1,
                        $"Invalid key declaration: {lineCopy.TruncatePretty(50)} ({ex.Message})", ex);
                }
                catch
                {
                    throw new LoliCodeParsingException(lineNumber, $"Invalid key declaration: {lineCopy.TruncatePretty(50)}");
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

        var statements = new List<StatementSyntax>
        {
            BlockSyntaxFactory.CreateMemberInvocation(
                Expr("data.Logger"),
                "LogHeader",
                Arg(Lit("CheckCondition"))).Stmt()
        };
        var banIfNoMatch = Settings["banIfNoMatch"];
        var nonEmpty = Keychains.Where(kc => kc.Keys.Count > 0).ToList();
        var continueBan = context.Settings.GeneralSettings.ContinueStatuses.Contains("BAN");
        var continueRetry = context.Settings.GeneralSettings.ContinueStatuses.Contains("RETRY");

        if (nonEmpty.Count == 0)
        {
            statements.Add(CSharpWriter.FromSettingSyntax(banIfNoMatch)
                .If(BlockSyntaxFactory.CreateStatusBlock("BAN", !continueBan)));
            statements.Add(CreateGlobalStatusCheck("CheckGlobalBanKeys", "BAN", !continueBan));
            statements.Add(CreateGlobalStatusCheck("CheckGlobalRetryKeys", "RETRY", !continueRetry));
            return statements;
        }

        StatementSyntax? tail = banIfNoMatch switch
        {
            { InputMode: SettingInputMode.Fixed, FixedSetting: BoolSetting { Value: true } }
                => BlockSyntaxFactory.CreateStatusBlock("BAN", !continueBan),
            { InputMode: SettingInputMode.Fixed } => null,
            _ => CSharpWriter.FromSettingSyntax(banIfNoMatch)
                .If(BlockSyntaxFactory.CreateStatusBlock("BAN", !continueBan))
        };

        for (var i = nonEmpty.Count - 1; i >= 0; i--)
        {
            var keychain = nonEmpty[i];
            var ifStatement = BuildKeychainCondition(keychain).If(
                BlockSyntaxFactory.CreateStatusBlock(
                    keychain.ResultStatus,
                    !context.Settings.GeneralSettings.ContinueStatuses.Contains(keychain.ResultStatus)));

            if (tail is not null)
            {
                ifStatement = ifStatement.WithElse(SyntaxFactory.ElseClause(tail));
            }

            tail = ifStatement;
        }

        if (tail is not null)
        {
            statements.Add(tail);
        }

        statements.Add(CreateGlobalStatusCheck("CheckGlobalBanKeys", "BAN", !continueBan));
        statements.Add(CreateGlobalStatusCheck("CheckGlobalRetryKeys", "RETRY", !continueRetry));
        return statements;
    }

    private static ExpressionSyntax BuildKeychainCondition(Keychain keychain)
    {
        var conditions = keychain.Keys.Select(CSharpWriter.ConvertKeySyntax).ToList();
        var combined = conditions[0];

        for (var i = 1; i < conditions.Count; i++)
        {
            combined = keychain.Mode switch
            {
                KeychainMode.OR => combined.Or(conditions[i]),
                KeychainMode.AND => combined.And(conditions[i]),
                _ => throw new InvalidOperationException("Invalid Keychain Mode")
            };
        }

        return combined;
    }

    private static IfStatementSyntax CreateGlobalStatusCheck(string methodName, string status, bool shouldReturn)
        => Id(methodName)
            .Call(Id("data"))
            .If(BlockSyntaxFactory.CreateStatusBlock(status, shouldReturn));
}
