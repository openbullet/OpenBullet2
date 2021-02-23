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
            : base(descriptor)
        {
            OutputVariable = descriptor.Id.Substring(0, 1).ToLower() + descriptor.Id[1..] + "Output";
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

        public override void FromLC(ref string script, ref int lineNumber)
        {
            // First parse the options that are common to every BlockInstance
            base.FromLC(ref script, ref lineNumber);

            using var reader = new StringReader(script);
            string line, lineCopy;

            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                lineNumber++;
                lineCopy = line;

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.StartsWith("=>"))
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
                if (declaredVariables.Contains(OutputVariable) || OutputVariable.StartsWith("globals."))
                {
                    writer.Write($"{OutputVariable} = ");
                }
                else
                {
                    if (!Disabled)
                        declaredVariables.Add(OutputVariable);

                    writer.Write($"{GetRuntimeReturnType()} {OutputVariable} = ");
                }
            }

            // If async, prepend the await keyword
            if ((Descriptor as AutoBlockDescriptor).Async)
                writer.Write("await ");

            // Append MethodName(data, param1, "param2", param3);
            var parameters = new List<string> { "data" }
                .Concat(Settings.Values.Select(s => CSharpWriter.FromSetting(s)));

            writer.Write($"{Descriptor.Id}({string.Join(", ", parameters)})");

            if ((Descriptor as AutoBlockDescriptor).Async)
            {
                writer.WriteLine(".ConfigureAwait(false);");
            }
            else
            {
                writer.WriteLine(";");
            }

            if (IsCapture)
                writer.WriteLine($"data.MarkForCapture(nameof({OutputVariable}));");

            return writer.ToString();
        }

        // This is needed otherwise when we have blocks made in other plugins they might reference
        // types from different runtimes and our castings like .AsBool() or .AsInt() will throw a
        // RuntimeBinderException, so we cannot just write 'var' but we need to explicitly write the type.
        private string GetRuntimeReturnType() => Descriptor.ReturnType switch
        {
            VariableType.Bool => "bool",
            VariableType.ByteArray => "byte[]",
            VariableType.DictionaryOfStrings => "Dictionary<string, string>",
            VariableType.Float => "float",
            VariableType.Int => "int",
            VariableType.ListOfStrings => "List<string>",
            VariableType.String => "string",
            _ => throw new NotSupportedException()
        };
    }
}
