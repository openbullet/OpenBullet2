using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Keycheck;
using RuriLib.Models.Blocks.Parameters;
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
        {
            Descriptor = descriptor;
            Id = descriptor.Id;
            Label = descriptor.Name;
            ReadableName = descriptor.Name;

            Settings = Descriptor.Parameters.Select(p => p.ToBlockSetting()).ToList();
        }

        public override string ToLC()
        {
            /*
             *   KEYCHAIN SUCCESS OR
             *     STRINGKEY @myVariable Contains "abc"
             *     DICTKEY @data.COOKIES HasKey "my-cookie"
             *   KEYCHAIN FAIL AND
             *     LISTKEY @myList Contains "item"
             *     FLOATKEY 1 GreaterThan 2
             */

            using var writer = new LoliCodeWriter(base.ToLC());

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

        public override void FromLC(ref string script)
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
            base.FromLC(ref script);

            using var reader = new StringReader(script);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();

                if (line.StartsWith("KEYCHAIN"))
                {
                    var keychain = new Keychain();
                    LineParser.ParseToken(ref line);
                    keychain.ResultStatus = LineParser.ParseToken(ref line);
                    keychain.Mode = (KeychainMode)Enum.Parse(typeof(KeychainMode), LineParser.ParseToken(ref line));
                    Keychains.Add(keychain);
                }

                else if (Regex.IsMatch(line, "^[A-Z]+KEY "))
                {
                    var keyType = LineParser.ParseToken(ref line);

                    Key key = keyType switch
                    {
                        "BOOLKEY" => ParseBoolKey(ref line),
                        "STRINGKEY" => ParseStringKey(ref line),
                        "INTKEY" => ParseIntKey(ref line),
                        "FLOATKEY" => ParseFloatKey(ref line),
                        "LISTKEY" => ParseListKey(ref line),
                        "DICTKEY" => ParseDictKey(ref line),
                        _ => throw new NotSupportedException()
                    };

                    Keychains.Last().Keys.Add(key);
                }

                else
                {
                    LoliCodeParser.ParseSetting(ref line, Settings, Descriptor);
                }
            }
        }

        private BoolKey ParseBoolKey(ref string line)
        {
            var key = new BoolKey();
            LoliCodeParser.ParseSettingValue(ref line, key.Left, new BoolParameter());
            key.Comparison = (BoolComparison)Enum.Parse(typeof(BoolComparison), LineParser.ParseToken(ref line));
            LoliCodeParser.ParseSettingValue(ref line, key.Right, new BoolParameter());
            return key;
        }

        private StringKey ParseStringKey(ref string line)
        {
            var key = new StringKey();
            LoliCodeParser.ParseSettingValue(ref line, key.Left, new StringParameter());
            key.Comparison = (StrComparison)Enum.Parse(typeof(StrComparison), LineParser.ParseToken(ref line));
            LoliCodeParser.ParseSettingValue(ref line, key.Right, new StringParameter());
            return key;
        }

        private IntKey ParseIntKey(ref string line)
        {
            var key = new IntKey();
            LoliCodeParser.ParseSettingValue(ref line, key.Left, new IntParameter());
            key.Comparison = (NumComparison)Enum.Parse(typeof(NumComparison), LineParser.ParseToken(ref line));
            LoliCodeParser.ParseSettingValue(ref line, key.Right, new IntParameter());
            return key;
        }

        private FloatKey ParseFloatKey(ref string line)
        {
            var key = new FloatKey();
            LoliCodeParser.ParseSettingValue(ref line, key.Left, new FloatParameter());
            key.Comparison = (NumComparison)Enum.Parse(typeof(NumComparison), LineParser.ParseToken(ref line));
            LoliCodeParser.ParseSettingValue(ref line, key.Right, new FloatParameter());
            return key;
        }

        private ListKey ParseListKey(ref string line)
        {
            var key = new ListKey();
            LoliCodeParser.ParseSettingValue(ref line, key.Left, new ListOfStringsParameter());
            key.Comparison = (ListComparison)Enum.Parse(typeof(ListComparison), LineParser.ParseToken(ref line));
            LoliCodeParser.ParseSettingValue(ref line, key.Right, new StringParameter());
            return key;
        }

        private DictionaryKey ParseDictKey(ref string line)
        {
            var key = new DictionaryKey();
            LoliCodeParser.ParseSettingValue(ref line, key.Left, new DictionaryOfStringsParameter());
            key.Comparison = (DictComparison)Enum.Parse(typeof(DictComparison), LineParser.ParseToken(ref line));
            LoliCodeParser.ParseSettingValue(ref line, key.Right, new StringParameter());
            return key;
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
            var banIfNoMatch = Settings.First(s => s.Name == "banIfNoMatch");
            var nonEmpty = Keychains.Where(kc => kc.Keys.Count > 0).ToList();

            // If there are no keychains
            if (nonEmpty.Count == 0)
            {
                writer.WriteLine($"if ({CSharpWriter.FromSetting(banIfNoMatch)})");

                if (settings.GeneralSettings.ContinueStatuses.Contains("BAN"))
                    writer.WriteLine("  data.STATUS = \"BAN\";");

                else
                    writer.WriteLine("  { data.STATUS = \"BAN\"; return; }");

                return writer.ToString();
            }

            // Write all the keychains
            for (int i = 0; i < nonEmpty.Count; i++)
            {
                var keychain = nonEmpty[i];

                if (i == 0)
                    writer.Write("if (");
                else
                    writer.Write("else if (");

                var conditions = keychain.Keys.Select(k => ConvertKey(k));

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
                    writer.WriteLine($"  data.STATUS = \"{keychain.ResultStatus}\";");
               
                // Do not continue on this status (return)
                else
                    writer.WriteLine($"  {{ data.STATUS = \"{keychain.ResultStatus}\"; return; }}");
            }

            writer.WriteLine($"else if ({CSharpWriter.FromSetting(banIfNoMatch)})");

            if (settings.GeneralSettings.ContinueStatuses.Contains("BAN"))
                writer.WriteLine("  data.STATUS = \"BAN\";");

            else
                writer.WriteLine("  { data.STATUS = \"BAN\"; return; }");
            
            return writer.ToString();
        }

        private string ConvertKey(Key key)
        {
            string comparison = key switch
            {
                BoolKey x => $"BoolComparison.{x.Comparison}",
                StringKey x => $"StrComparison.{x.Comparison}",
                IntKey x => $"NumComparison.{x.Comparison}",
                FloatKey x => $"NumComparison.{x.Comparison}",
                ListKey x => $"ListComparison.{x.Comparison}",
                DictionaryKey x => $"DictComparison.{x.Comparison}",
                _ => throw new Exception("Unknown key type")
            };

            string left = CSharpWriter.FromSetting(key.Left);
            string right = CSharpWriter.FromSetting(key.Right);

            return $"Conditions.Check({left}, {comparison}, {right})";
        }
    }
}
