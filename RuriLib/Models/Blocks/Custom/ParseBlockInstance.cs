using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Parse;
using RuriLib.Models.Blocks.Parameters;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks.Custom
{
    public class ParseBlockInstance : BlockInstance
    {
        private string outputVariable = "parseOutput";
        public string OutputVariable
        {
            get => outputVariable;
            set => outputVariable = VariableNames.MakeValid(value);
        }

        public bool Recursive { get; set; } = false;
        public bool IsCapture { get; set; } = false;
        public ParseMode Mode { get; set; } = ParseMode.LR;

        public BlockSetting Input { get; set; } = BlockSettingFactory.CreateStringSetting("input", "", SettingInputMode.Variable);

        // LR
        public BlockSetting LeftDelim { get; set; } = BlockSettingFactory.CreateStringSetting("leftDelim");
        public BlockSetting RightDelim { get; set; } = BlockSettingFactory.CreateStringSetting("rightDelim");
        public BlockSetting CaseSensitive { get; set; } = new BlockSetting { Name = "caseSensitive", FixedSetting = new BoolSetting { Value = true } };

        // CSS
        public BlockSetting CssSelector { get; set; } = BlockSettingFactory.CreateStringSetting("cssSelector");
        public BlockSetting AttributeName { get; set; } = BlockSettingFactory.CreateStringSetting("attributeName", "innerHTML");

        // JSON
        public BlockSetting JToken { get; set; } = BlockSettingFactory.CreateStringSetting("jToken");

        // REGEX
        public BlockSetting Pattern { get; set; } = BlockSettingFactory.CreateStringSetting("pattern");
        public BlockSetting OutputFormat { get; set; } = BlockSettingFactory.CreateStringSetting("outputFormat");

        public ParseBlockInstance(ParseBlockDescriptor descriptor)
        {
            Descriptor = descriptor;
            Id = descriptor.Id;
            Label = descriptor.Name;
            ReadableName = descriptor.Name;
            Input.InputVariableName = "data.SOURCE";

            Settings = new List<BlockSetting>();
        }

        public override string ToLC()
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

            using var writer = new LoliCodeWriter(base.ToLC());
            
            if (Recursive)
                writer.AppendLine("RECURSIVE", 2);

            writer.AppendLine($"MODE:{Mode}", 2);
            writer.AppendSetting(Input);
            
            switch (Mode)
            {
                case ParseMode.LR:
                    writer
                        .AppendSetting(LeftDelim)
                        .AppendSetting(RightDelim)
                        .AppendSetting(CaseSensitive);
                    break;

                case ParseMode.CSS:
                    writer
                        .AppendSetting(CssSelector)
                        .AppendSetting(AttributeName);
                    break;

                case ParseMode.Json:
                    writer
                        .AppendSetting(JToken);
                    break;

                case ParseMode.Regex:
                    writer
                        .AppendSetting(Pattern)
                        .AppendSetting(OutputFormat);
                    break;
            }

            var isCap = IsCapture ? "CAP" : "VAR";
            writer.AppendLine($"=> {isCap} @{OutputVariable}");

            return writer.ToString();
        }

        public override void FromLC(ref string script)
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

            // First parse the options that are common to every BlockInstance
            base.FromLC(ref script);

            using var reader = new StringReader(script);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();

                if (line.StartsWith("RECURSIVE"))
                    Recursive = true;

                else if (line.StartsWith("input"))
                {
                    line = line.Replace("input = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, Input, new StringParameter());
                }

                else if (line.StartsWith("MODE"))
                    Mode = (ParseMode)Enum.Parse(typeof(ParseMode), Regex.Match(line, "MODE:([A-Za-z]+)").Groups[1].Value);

                // TODO: Refactor

                // LR
                else if (line.StartsWith("leftDelim"))
                {
                    line = line.Replace("leftDelim = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, LeftDelim, new StringParameter());
                }

                else if (line.StartsWith("rightDelim"))
                {
                    line = line.Replace("rightDelim = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, RightDelim, new StringParameter());
                }
                
                else if (line.StartsWith("caseSensitive"))
                {
                    line = line.Replace("caseSensitive = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, CaseSensitive, new BoolParameter());
                }
                
                // CSS
                else if (line.StartsWith("cssSelector"))
                {
                    line = line.Replace("cssSelector = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, CssSelector, new StringParameter());
                }
                    
                else if (line.StartsWith("attributeName"))
                {
                    line = line.Replace("attributeName = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, AttributeName, new StringParameter());
                }

                // JSON
                else if (line.StartsWith("jToken"))
                {
                    line = line.Replace("jToken = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, JToken, new StringParameter());
                }

                // REGEX
                else if (line.StartsWith("pattern"))
                {
                    line = line.Replace("pattern = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, Pattern, new StringParameter());
                }

                else if (line.StartsWith("outputFormat"))
                {
                    line = line.Replace("outputFormat = ", "");
                    LoliCodeParser.ParseSettingValue(ref line, OutputFormat, new StringParameter());
                }

                else if (line.StartsWith("=>"))
                {
                    var match = Regex.Match(line, "^=> ([A-Za-z]{3}) (.*)$");
                    IsCapture = match.Groups[1].Value.Equals("CAP", StringComparison.OrdinalIgnoreCase);
                    OutputVariable = match.Groups[2].Value.Trim()[1..];
                }
            }
        }

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            using var writer = new StringWriter();
            
            if (definedVariables.Contains(OutputVariable))
            {
                writer.Write($"{OutputVariable} = ");
            }
            else
            {
                if (!Disabled)
                    definedVariables.Add(OutputVariable);

                writer.Write($"var {OutputVariable} = ");
            }

            switch (Mode)
            {
                case ParseMode.LR:
                    writer.Write("ParseBetweenStrings");
                    break;

                case ParseMode.CSS:
                    writer.Write("QueryCssSelector");
                    break;

                case ParseMode.Json:
                    writer.Write("QueryJsonToken");
                    break;

                case ParseMode.Regex:
                    writer.Write("MatchRegexGroups");
                    break;
            }

            if (Recursive)
                writer.Write("Recursive");

            writer.Write("(data, ");
            writer.Write(CSharpWriter.FromSetting(Input) + ", ");

            switch (Mode)
            {
                case ParseMode.LR:
                    writer.Write(CSharpWriter.FromSetting(LeftDelim) + ", ");
                    writer.Write(CSharpWriter.FromSetting(RightDelim) + ", ");
                    writer.Write(CSharpWriter.FromSetting(CaseSensitive));
                    break;

                case ParseMode.CSS:
                    writer.Write(CSharpWriter.FromSetting(CssSelector) + ", ");
                    writer.Write(CSharpWriter.FromSetting(AttributeName));
                    break;

                case ParseMode.Json:
                    writer.Write(CSharpWriter.FromSetting(JToken));
                    break;

                case ParseMode.Regex:
                    writer.Write(CSharpWriter.FromSetting(Pattern) + ", ");
                    writer.Write(CSharpWriter.FromSetting(OutputFormat));
                    break;
            }

            writer.WriteLine(");");

            if (IsCapture)
                writer.WriteLine($"data.MarkForCapture(nameof({OutputVariable}));");

            return writer.ToString();
        }
    }
}
