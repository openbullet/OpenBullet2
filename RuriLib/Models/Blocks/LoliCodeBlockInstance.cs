using AngleSharp.Text;
using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks
{
    public class LoliCodeBlockInstance : BlockInstance
    {
        private readonly string validTokenRegex = "[A-Za-z][A-Za-z0-9_]*";
        public string Script { get; set; }
        
        public LoliCodeBlockInstance(LoliCodeBlockDescriptor descriptor)
            : base(descriptor)
        {
            
        }

        public override string ToLC() => Script;

        public override void FromLC(ref string script, ref int lineNumber) 
        {
            Script = script;
            lineNumber += script.CountLines();
        }

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

            // CODE LABEL
            // #MYLABEL => MYLABEL:
            if ((match = Regex.Match(input, $"^#({validTokenRegex})$")).Success)
            {
                return $"{match.Groups[1].Value}:";
            }

            // JUMP
            // JUMP #MYLABEL => goto MYLABEL;
            if ((match = Regex.Match(input, $"^JUMP #({validTokenRegex})$")).Success)
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
            if ((match = Regex.Match(input, $"^REPEAT ([0-9]+)$")).Success)
            {
                var i = VariableNames.RandomName();
                return $"for (var {i} = 0; {i} < {match.Groups[1].Value}; {i}++){System.Environment.NewLine}{{";
            }

            // FOREACH
            // FOREACH v IN list => foreach (var v in list) {
            if ((match = Regex.Match(input, $"^FOREACH ({validTokenRegex}) IN ({validTokenRegex})$")).Success)
            {
                return $"foreach (var {match.Groups[1].Value} in {match.Groups[2].Value}){System.Environment.NewLine}{{";
            }

            // LOG
            // LOG myVar => data.Logger.Log(myVar);
            if ((match = Regex.Match(input, $"^LOG (.+)$")).Success)
            {
                return $"data.Logger.Log({match.Groups[1].Value});";
            }

            // CLOG
            // CLOG Tomato "hello" => data.Logger.Log("hello", LogColors.Tomato);
            if ((match = Regex.Match(input, $"^CLOG ([A-Za-z]+) (.+)$")).Success)
            {
                return $"data.Logger.Log({match.Groups[2].Value}, LogColors.{match.Groups[1].Value});";
            }

            // WHILE
            // WHILE a < b => while (a < b) {
            if ((match = Regex.Match(input, $"^WHILE (.+)$")).Success)
            {
                var line = match.Groups[1].Value.Trim();
                if (LoliCodeParser.KeyTypes.Any(t => line.StartsWith(t)))
                {
                    var keyType = LineParser.ParseToken(ref line);
                    var key = LoliCodeParser.ParseKey(ref line, keyType);
                    return $"while ({CSharpWriter.ConvertKey(key)}){System.Environment.NewLine}{{";
                }
                else
                {
                    return $"while ({line}){System.Environment.NewLine}{{";
                }
            }

            // IF
            // IF a < b => if (a < b) {
            if ((match = Regex.Match(input, $"^IF (.+)$")).Success)
            {
                var line = match.Groups[1].Value.Trim();
                if (LoliCodeParser.KeyTypes.Any(t => line.StartsWith(t)))
                {
                    var keyType = LineParser.ParseToken(ref line);
                    var key = LoliCodeParser.ParseKey(ref line, keyType);
                    return $"if ({CSharpWriter.ConvertKey(key)}){System.Environment.NewLine}{{";
                }
                else
                {
                    return $"if ({line}){System.Environment.NewLine}{{";
                }
            }

            // ELSE
            // ELSE => } else {
            if (input == "ELSE")
            {
                return $"}}{System.Environment.NewLine}else{System.Environment.NewLine}{{";
            }

            // ELSE IF
            // ELSE IF a < b => } else if (a < b) {
            if ((match = Regex.Match(input, $"ELSE IF (.+)$")).Success)
            {
                var line = match.Groups[1].Value.Trim();
                if (LoliCodeParser.KeyTypes.Any(t => line.StartsWith(t)))
                {
                    var keyType = LineParser.ParseToken(ref line);
                    var key = LoliCodeParser.ParseKey(ref line, keyType);
                    return $"}}{System.Environment.NewLine}else if ({CSharpWriter.ConvertKey(key)}){System.Environment.NewLine}{{";
                }
                else
                {
                    return $"}}{System.Environment.NewLine}else if ({line}){System.Environment.NewLine}{{";
                }
            }

            // TRY
            // TRY => try {
            if (input == "TRY")
            {
                return $"try{System.Environment.NewLine}{{";
            }

            // CATCH
            // CATCH => } catch {
            if (input == "CATCH")
            {
                return $"}}{System.Environment.NewLine}catch{System.Environment.NewLine}{{";
            }

            throw new NotSupportedException();
        }
    }
}
