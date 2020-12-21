using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks
{
    /// <summary>
    /// An instance of a block that was auto generated from exposed methods.
    /// </summary>
    public class AutoBlockInstance : BlockInstance
    {
        private string outputVariable = "output";
        public string OutputVariable
        {
            get => outputVariable;
            set => outputVariable = VariableNames.MakeValid(value);
        }

        public bool IsCapture { get; set; } = false;

        public AutoBlockInstance(AutoBlockDescriptor descriptor)
        {
            Descriptor = descriptor;
            Id = descriptor.Id;
            Label = descriptor.Name;
            ReadableName = descriptor.Name;
            OutputVariable = descriptor.Id.Substring(0, 1).ToLower() + descriptor.Id.Substring(1) + "Output";

            // Here convert parameters to settings
            Settings = Descriptor.Parameters.Values.Select(p => p.ToBlockSetting())
                .ToDictionary(p => p.Name, p => p);
        }

        public override string ToLC()
        {
            /*
             *   SettingName = "my value"
             *   SettingName = 0
             *   SettingName = @myVariable
             */

            using var writer = new LoliCodeWriter(base.ToLC());

            var outVarKind = IsCapture ? "CAP" : "VAR";

            // Write the output variable
            if (Descriptor.ReturnType.HasValue)
                writer.AppendLine($"=> {outVarKind} @{OutputVariable}", 2);

            return writer.ToString();
        }

        public override void FromLC(ref string script)
        {
            // First parse the options that are common to every BlockInstance
            base.FromLC(ref script);

            using var reader = new StringReader(script);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                line = line.Trim();

                if (line.StartsWith("=>"))
                {
                    var match = Regex.Match(line, "^=> ([A-Za-z]{3}) (.*)$");
                    IsCapture = match.Groups[1].Value.Equals("CAP", StringComparison.OrdinalIgnoreCase);
                    OutputVariable = match.Groups[2].Value.Trim()[1..];
                }
                else
                {
                    LoliCodeParser.ParseSetting(ref line, Settings, Descriptor);
                }
            }
        }

        public override string ToCSharp(List<string> declaredVariables, ConfigSettings settings)
        {
            // If disabled /* code here */

            /*
             * With return type:
             * var myVar = MethodName(data, param1, param2 ...);
             * 
             * Async:
             * await MethodName(data, param1, param2 ...);
             * 
             */

            using var writer = new StringWriter();

            // If not void, do variable assignment
            if (Descriptor.ReturnType.HasValue)
            {
                if (declaredVariables.Contains(OutputVariable))
                {
                    writer.Write($"{OutputVariable} = ");
                }
                else
                {
                    if (!Disabled)
                        declaredVariables.Add(OutputVariable);

                    writer.Write($"var {OutputVariable} = ");
                }
            }

            // If async, prepend the await keyword
            if ((Descriptor as AutoBlockDescriptor).Async)
                writer.Write("await ");

            // Append MethodName(data, param1, "param2", param3);
            var parameters = new List<string> { "data" }
                .Concat(Settings.Values.Select(s => CSharpWriter.FromSetting(s)));

            writer.WriteLine($"{Descriptor.Id}({string.Join(", ", parameters)});");

            if (IsCapture)
                writer.WriteLine($"data.MarkForCapture(nameof({OutputVariable}));");

            return writer.ToString();
        }
    }
}
