using RuriLib.Helpers.LoliCode;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks
{
    public class LoliCodeBlockInstance : BlockInstance
    {
        private readonly string validTokenRegex = "[A-Za-z][A-Za-z0-9_]*";
        public string Script { get; set; }
        
        public LoliCodeBlockInstance(LoliCodeBlockDescriptor descriptor)
        {
            Descriptor = descriptor;
            Id = descriptor.Id;
            Label = descriptor.Name;
            ReadableName = descriptor.Name;
        }

        public override void FromLC(ref string script) { Script = script; }
        public override string ToLC() => Script;

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            using var reader = new StringReader(Script);
            using var writer = new StringWriter();
            string line, trimmedLine;

            while ((line = reader.ReadLine()) != null)
            {
                trimmedLine = line.Trim();

                // Try to read it as a LoliCode-exclusive statement
                try
                {
                    writer.WriteLine(TranspileStatement(trimmedLine));
                }

                // If it failed, we assume what is written is bare C# so we just copy it over (untrimmed)
                catch (NotSupportedException)
                {
                    writer.WriteLine(line);
                }
            }

            return writer.ToString();
        }

        private string TranspileStatement(string input)
        {
            Match match;

            // Code Label
            if ((match = Regex.Match(input, $"^#({validTokenRegex})$")).Success)
            {
                return $"{match.Groups[1].Value}:";
            }

            // Jump
            if ((match = Regex.Match(input, $"^JUMP #({validTokenRegex})$")).Success)
            {
                return $"goto {match.Groups[1].Value};";
            }

            throw new NotSupportedException();
        }
    }
}
