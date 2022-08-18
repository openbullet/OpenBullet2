using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Conditions.Comparisons;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks.Custom
{
    public class KeycheckBlockInstance : BlockInstance
    {
        public List<Keychain> Keychains { get; set; } = new List<Keychain>();

        public KeycheckBlockInstance(KeycheckBlockDescriptor descriptor)
            : base(descriptor)
        {
            
        }

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
                    (string keyName, string comparison) = key switch
                    {
                        BoolKey x => ("BOOLKEY", x.Comparison.ToString()),
                        StringKey x => ("STRINGKEY", x.Comparison.ToString()),
                        IntKey x => ("INTKEY", x.Comparison.ToString()),
                        FloatKey x => ("FLOATKEY", x.Comparison.ToString()),
                        DictionaryKey x => ("DICTKEY", x.Comparison.ToString()),
                        ListKey x => ("LISTKEY", x.Comparison.ToString()),
                        _ => throw new Exception("Unknown key type")
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

            // First parse the options that are common to every BlockInstance
            base.FromLC(ref script, ref lineNumber);

            using var reader = new StringReader(script);

            while (reader.ReadLine() is { } line)
            {
                line = line.Trim();
                var lineCopy = line;
                lineNumber++;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("KEYCHAIN"))
                {
                    try
                    {
                        var keychain = new Keychain();
                        LineParser.ParseToken(ref line);
                        keychain.ResultStatus = LineParser.ParseToken(ref line);
                        keychain.Mode = Enum.Parse<KeychainMode>(LineParser.ParseToken(ref line));
                        Keychains.Add(keychain);
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
                        var keyType = LineParser.ParseToken(ref line);
                        Keychains.Last().Keys.Add(LoliCodeParser.ParseKey(ref line, keyType));
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
                    catch
                    {
                        throw new LoliCodeParsingException(lineNumber, $"Could not parse the setting: {lineCopy.TruncatePretty(50)}");
                    }
                }
            }
        }

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            /*
             *   if (Conditions.Check(myVar, StrComparison.Contains, "hello"))
             *     data.STATUS = "SUCCESS";
             *     
             *   else if (Conditions.Check(myList, ListComparison.Contains, "item") || Conditions.Check(data.COOKIES, DictComparison.HasKey, "name"))
             *     { data.STATUS = "FAIL"; return; }
             *     
             *   else if (myBool)
             *     { data.STATUS = "BAN"; return; }
             */

            using var writer = new StringWriter();
            var banIfNoMatch = Settings["banIfNoMatch"];
            var nonEmpty = Keychains.Where(kc => kc.Keys.Count > 0).ToList();

            // If there are no keychains
            if (nonEmpty.Count == 0)
            {
                writer.WriteLine($"if ({CSharpWriter.FromSetting(banIfNoMatch)})");

                if (settings.GeneralSettings.ContinueStatuses.Contains("BAN"))
                {
                    writer.WriteLine(" { data.STATUS = \"BAN\"; }");
                    writer.WriteLine("if (CheckGlobalBanKeys(data)) { data.STATUS = \"BAN\"; }");
                }
                else
                {
                    writer.WriteLine("  { data.STATUS = \"BAN\"; return; }");
                    writer.WriteLine("if (CheckGlobalBanKeys(data)) { data.STATUS = \"BAN\"; return; }");
                }

                if (settings.GeneralSettings.ContinueStatuses.Contains("RETRY"))
                {
                    writer.WriteLine("if (CheckGlobalRetryKeys(data)) { data.STATUS = \"RETRY\"; }");
                }
                else
                {
                    writer.WriteLine("if (CheckGlobalRetryKeys(data)) { data.STATUS = \"RETRY\"; return; }");
                }

                return writer.ToString();
            }

            // Write all the keychains
            for (var i = 0; i < nonEmpty.Count; i++)
            {
                var keychain = nonEmpty[i];

                if (i == 0)
                {
                    writer.Write("if (");
                }
                else
                {
                    writer.Write("else if (");
                }

                var conditions = keychain.Keys.Select(CSharpWriter.ConvertKey);

                var chainedCondition = keychain.Mode switch
                {
                    KeychainMode.OR => string.Join(" || ", conditions),
                    KeychainMode.AND => string.Join(" && ", conditions),
                    _ => throw new Exception("Invalid Keychain Mode")
                };

                writer.Write(chainedCondition);
                writer.WriteLine(")");

                // Continue on this status
                if (settings.GeneralSettings.ContinueStatuses.Contains(keychain.ResultStatus))
                {
                    writer.WriteLine($" {{ data.STATUS = \"{keychain.ResultStatus}\"; }}");
                }

                // Do not continue on this status (return)
                else
                {
                    writer.WriteLine($"  {{ data.STATUS = \"{keychain.ResultStatus}\"; return; }}");
                }
            }

            // The whole purpose of this is to make the code a bit prettier
            if (banIfNoMatch.InputMode == SettingInputMode.Fixed)
            {
                if (((BoolSetting)banIfNoMatch.FixedSetting).Value)
                {
                    writer.WriteLine("else");

                    if (settings.GeneralSettings.ContinueStatuses.Contains("BAN"))
                    {
                        writer.WriteLine(" { data.STATUS = \"BAN\"; }");
                    }
                    else
                    {
                        writer.WriteLine("  { data.STATUS = \"BAN\"; return; }");
                    }
                }
                else
                {

                }
            }
            else
            {
                writer.WriteLine($"else if ({CSharpWriter.FromSetting(banIfNoMatch)})");

                if (settings.GeneralSettings.ContinueStatuses.Contains("BAN"))
                {
                    writer.WriteLine(" { data.STATUS = \"BAN\"; }");
                }
                else
                {
                    writer.WriteLine("  { data.STATUS = \"BAN\"; return; }");
                }
            }

            // Check global ban keys
            if (settings.GeneralSettings.ContinueStatuses.Contains("BAN"))
            {
                writer.WriteLine("if (CheckGlobalBanKeys(data)) { data.STATUS = \"BAN\"; }");
            }
            else
            {
                writer.WriteLine("if (CheckGlobalBanKeys(data)) { data.STATUS = \"BAN\"; return; }");
            }

            if (settings.GeneralSettings.ContinueStatuses.Contains("RETRY"))
            {
                writer.WriteLine("if (CheckGlobalRetryKeys(data)) { data.STATUS = \"RETRY\"; }");
            }
            else
            {
                writer.WriteLine("if (CheckGlobalRetryKeys(data)) { data.STATUS = \"RETRY\"; return; }");
            }

            return writer.ToString();
        }
    }
}
