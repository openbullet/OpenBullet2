using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Parse;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
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
        public bool Safe { get; set; } = false;
        public ParseMode Mode { get; set; } = ParseMode.LR;

        public ParseBlockInstance(ParseBlockDescriptor descriptor)
            : base(descriptor)
        {
            
        }

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
                writer.AppendLine("RECURSIVE", 2);

            writer.AppendLine($"MODE:{Mode}", 2);
            
            var isCap = IsCapture ? "CAP" : "VAR";
            writer.AppendLine($"=> {isCap} @{OutputVariable}", 2);

            return writer.ToString();
        }

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

            // First parse the options that are common to every BlockInstance
            base.FromLC(ref script, ref lineNumber);

            using var reader = new StringReader(script);

            while (reader.ReadLine() is { } line)
            {
                line = line.Trim();
                lineNumber++;
                var lineCopy = line;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("SAFE"))
                {
                    Safe = true;
                    continue;
                }

                if (line.StartsWith("RECURSIVE"))
                    Recursive = true;

                else if (line.StartsWith("MODE"))
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

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            using var writer = new StringWriter();
            var outputType = Recursive ? "List<string>" : "string";
            var defaultReturnValue = Recursive ? "new List<string>()" : "string.Empty";

            // Safe mode, wrap method in try/catch but declare variable outside of it
            if (Safe)
            {
                // Only do this if we haven't declared the variable yet!
                if (!definedVariables.Contains(OutputVariable) && !OutputVariable.StartsWith("globals."))
                {
                    if (!Disabled)
                        definedVariables.Add(OutputVariable);

                    writer.WriteLine($"{outputType} {OutputVariable} = {defaultReturnValue};");
                }

                writer.WriteLine("try {");

                // Here we already know the variable exists so we just do the assignment
                writer.Write($"{OutputVariable} = ");

                WriteParseMethod(writer);

                writer.WriteLine("} catch (Exception safeException) {");
                writer.WriteLine("data.ERROR = safeException.PrettyPrint();");
                writer.WriteLine("data.Logger.Log($\"[SAFE MODE] Exception caught and saved to data.ERROR: {data.ERROR}\", LogColors.Tomato); }");
            }
            else
            {
                if (definedVariables.Contains(OutputVariable) || OutputVariable.StartsWith("globals."))
                {
                    writer.Write($"{OutputVariable} = ");
                }
                else
                {
                    if (!Disabled)
                        definedVariables.Add(OutputVariable);

                    writer.Write($"{outputType} {OutputVariable} = ");
                }

                WriteParseMethod(writer);
            }

            return writer.ToString();
        }

        private void WriteParseMethod(StringWriter writer)
        {
            switch (Mode)
            {
                case ParseMode.LR:
                    writer.Write("ParseBetweenStrings");
                    break;

                case ParseMode.CSS:
                    writer.Write("QueryCssSelector");
                    break;

                case ParseMode.XPath:
                    writer.Write("QueryXPath");
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
            writer.Write(CSharpWriter.FromSetting(Settings["input"]) + ", ");

            switch (Mode)
            {
                case ParseMode.LR:
                    writer.Write(CSharpWriter.FromSetting(Settings["leftDelim"]) + ", ");
                    writer.Write(CSharpWriter.FromSetting(Settings["rightDelim"]) + ", ");
                    writer.Write(CSharpWriter.FromSetting(Settings["caseSensitive"]) + ", ");

                    break;

                case ParseMode.CSS:
                    writer.Write(CSharpWriter.FromSetting(Settings["cssSelector"]) + ", ");
                    writer.Write(CSharpWriter.FromSetting(Settings["attributeName"]) + ", ");
                    break;

                case ParseMode.XPath:
                    writer.Write(CSharpWriter.FromSetting(Settings["xPath"]) + ", ");
                    writer.Write(CSharpWriter.FromSetting(Settings["attributeName"]) + ", ");
                    break;

                case ParseMode.Json:
                    writer.Write(CSharpWriter.FromSetting(Settings["jToken"]) + ", ");
                    break;

                case ParseMode.Regex:
                    writer.Write(CSharpWriter.FromSetting(Settings["pattern"]) + ", ");
                    writer.Write(CSharpWriter.FromSetting(Settings["outputFormat"]) + ", ");
                    writer.Write(CSharpWriter.FromSetting(Settings["multiLine"]) + ", ");
                    break;
            }

            writer.Write(CSharpWriter.FromSetting(Settings["prefix"]) + ", ");
            writer.Write(CSharpWriter.FromSetting(Settings["suffix"]) + ", ");
            writer.Write(CSharpWriter.FromSetting(Settings["urlEncodeOutput"]));
            writer.WriteLine(");");

            writer.WriteLine($"data.LogVariableAssignment(nameof({OutputVariable}));");

            if (IsCapture)
            {
                writer.WriteLine($"data.MarkForCapture(nameof({OutputVariable}));");
            }
        }
    }
}
