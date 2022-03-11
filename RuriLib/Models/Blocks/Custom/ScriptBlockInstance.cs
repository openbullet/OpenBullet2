using RuriLib.Exceptions;
using RuriLib.Extensions;
using RuriLib.Functions.Conversion;
using RuriLib.Functions.Crypto;
using RuriLib.Helpers;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.LoliCode;
using RuriLib.Models.Blocks.Custom.Script;
using RuriLib.Models.Configs;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Blocks.Custom
{
    public class ScriptBlockInstance : BlockInstance
    {
        public string Script { get; set; } = @"var result = x + y;";

        public List<OutputVariable> OutputVariables { get; set; } = new List<OutputVariable> 
        { 
            new OutputVariable 
            {
                Name = "result",
                Type = VariableType.Int
            } 
        };

        public string InputVariables { get; set; } = "x,y";
        public Interpreter Interpreter { get; set; } = Interpreter.Jint;

        public ScriptBlockInstance(ScriptBlockDescriptor descriptor)
            : base(descriptor)
        {
            
        }

        public override string ToLC(bool printDefaultParams = false)
        {
            /*
             *   INTERPRETER:Jint
             *   INPUT x,y
             *   BEGIN SCRIPT
             *   var result = x + y;
             *   END SCRIPT
             *   OUTPUT Int result
             */

            using var writer = new LoliCodeWriter(base.ToLC(printDefaultParams));
            writer.WriteLine($"INTERPRETER:{Interpreter}");
            writer.WriteLine($"INPUT {InputVariables}");
            writer.WriteLine("BEGIN SCRIPT");
            writer.WriteLine(Regex.Replace(Script, $"(\r\n)*$", ""));
            writer.WriteLine("END SCRIPT");

            foreach (var output in OutputVariables)
                writer.WriteLine($"OUTPUT {output.Type} @{output.Name}");

            return writer.ToString();
        }


        public override void FromLC(ref string script, ref int lineNumber)
        {
            // First parse the options that are common to every BlockInstance
            base.FromLC(ref script, ref lineNumber);

            using var reader = new StringReader(script);
            using var writer = new StringWriter();
            string line;

            // Parse the interpreter
            line = reader.ReadLine();
            lineNumber++;

            try
            {
                Interpreter = Enum.Parse<Interpreter>(Regex.Match(line, "INTERPRETER:([^ ]+)$").Groups[1].Value);
            }
            catch
            {
                throw new LoliCodeParsingException(lineNumber, $"Invalid interpreter definition: {line.TruncatePretty(50)}");
            }

            // Parse the input variables
            line = reader.ReadLine();
            lineNumber++;

            try
            {
                InputVariables = Regex.Match(line, "INPUT (.*)$").Groups[1].Value;
            }
            catch
            {
                throw new LoliCodeParsingException(lineNumber, "Invalid input variables definition");
            }

            reader.ReadLine(); // Read BEGIN SCRIPT
            lineNumber++;
            
            while ((line = reader.ReadLine()) != null && line != "END SCRIPT")
            {
                lineNumber++;
                writer.WriteLine(line);
            }

            Script = writer.ToString();
            Script = Regex.Replace(Script, $"(\r\n)*$", ""); // Remove blank lines at the end except one

            OutputVariables = new List<OutputVariable>();
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                var match = Regex.Match(line, "OUTPUT ([^ ]+) @([^ ]+)$");
                OutputVariables.Add(
                    new OutputVariable
                    {
                        Type = Enum.Parse<VariableType>(match.Groups[1].Value),
                        Name = match.Groups[2].Value
                    });
            }
        }

        public override string ToCSharp(List<string> definedVariables, ConfigSettings settings)
        {
            using var writer = new StringWriter();
            string scriptHash, scriptPath;
            var resultName = "tmp_" + VariableNames.RandomName(6);
            var engineName = "tmp_" + VariableNames.RandomName(6);
            var scopeName = "tmp_" + VariableNames.RandomName(6);

            switch (Interpreter)
            {
                case Interpreter.Jint:

                    scriptHash = HexConverter.ToHexString(Crypto.MD5(Encoding.UTF8.GetBytes(Script)));
                    scriptPath = $"Scripts/{scriptHash}.{GetScriptFileExtension(Interpreter)}";

                    if (!Directory.Exists("Scripts"))
                        Directory.CreateDirectory("Scripts");

                    if (!File.Exists(scriptPath))
                        File.WriteAllText(scriptPath, Script);
                    
                    writer.WriteLine($"var {engineName} = new Engine();");

                    if (!string.IsNullOrWhiteSpace(InputVariables))
                    {
                        foreach (var input in InputVariables.Split(','))
                        {
                            writer.WriteLine($"{engineName}.SetValue(nameof({input}), {input});");
                        }
                    }

                    writer.WriteLine($"{engineName} = InvokeJint(data, {engineName}, \"{scriptPath}\");");

                    foreach (var output in OutputVariables)
                    {
                        if (!definedVariables.Contains(output.Name))
                            writer.Write($"{ToCSharpType(output.Type)} ");

                        writer.WriteLine($"{output.Name} = {engineName}.Global.GetProperty(\"{output.Name}\").Value.{GetJintMethod(output.Type)};");
                    }

                    break;

                case Interpreter.NodeJS:
                    var nodeScript = @$"module.exports = async ({MakeInputs()}) => {{
{Script}
var noderesult = {{
{MakeNodeObject()}
}};
return noderesult;
}}";

                    scriptHash = HexConverter.ToHexString(Crypto.MD5(Encoding.UTF8.GetBytes(nodeScript)));
                    scriptPath = $"Scripts/{scriptHash}.{GetScriptFileExtension(Interpreter)}";

                    if (!Directory.Exists("Scripts"))
                        Directory.CreateDirectory("Scripts");

                    if (!File.Exists(scriptPath))
                        File.WriteAllText(scriptPath, nodeScript);

                    writer.WriteLine($"var {resultName} = await InvokeNode<dynamic>(data, \"{scriptPath}\", new object[] {{ {InputVariables} }});");

                    foreach (var output in OutputVariables)
                    {
                        if (!definedVariables.Contains(output.Name))
                            writer.Write($"{ToCSharpType(output.Type)} ");

                        writer.WriteLine($"{output.Name} = {resultName}.GetProperty(\"{output.Name}\").{GetNodeMethod(output.Type)};");
                    }

                    break;

                case Interpreter.IronPython:
                    
                    scriptHash = HexConverter.ToHexString(Crypto.MD5(Encoding.UTF8.GetBytes(Script)));
                    scriptPath = $"Scripts/{scriptHash}.{GetScriptFileExtension(Interpreter)}";

                    if (!Directory.Exists("Scripts"))
                        Directory.CreateDirectory("Scripts");

                    if (!File.Exists(scriptPath))
                        File.WriteAllText(scriptPath, Script);

                    writer.WriteLine($"var {scopeName} = GetIronPyScope(data);");

                    if (!string.IsNullOrWhiteSpace(InputVariables))
                    {
                        foreach (var input in InputVariables.Split(','))
                        {
                            writer.WriteLine($"{scopeName}.SetVariable(nameof({input}), {input});");
                        }
                    }

                    writer.WriteLine($"ExecuteIronPyScript(data, {scopeName}, \"{scriptPath}\");");
                    
                    foreach (var output in OutputVariables)
                    {
                        if (!definedVariables.Contains(output.Name))
                            writer.Write($"{ToCSharpType(output.Type)} ");

                        writer.WriteLine($"{output.Name} = {scopeName}" + output.Type switch
                        {
                            VariableType.ListOfStrings => $".GetVariable<IList<object>>(\"{output.Name}\").Cast<string>().ToList();",
                            VariableType.ByteArray => $".GetVariable<IList<object>>(\"{output.Name}\").Cast<byte>().ToArray();",
                            _ => $".GetVariable<{ToCSharpType(output.Type)}>(\"{output.Name}\");"
                        });
                    }

                    break;
            }

            foreach (var output in OutputVariables)
            {
                writer.WriteLine($"data.LogVariableAssignment(nameof({output.Name}));");
            }

            return writer.ToString();
        }

        private string GetNodeMethod(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => "GetBoolean()",
                VariableType.ByteArray => "GetBytesFromBase64()",
                VariableType.Float => "GetSingle()",
                VariableType.Int => "GetInt32()",
                VariableType.ListOfStrings => "EnumerateArray().Select(e => e.GetString()).ToList()",
                VariableType.String => "ToString()",
                _ => throw new NotImplementedException() // Dictionary not implemented yet
            };
        }

        private string GetJintMethod(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => "AsBoolean()",
                VariableType.ByteArray => "TryCast<byte[]>()",
                VariableType.Float => "AsNumber().ToSingle()",
                VariableType.Int => "AsNumber().ToInt()",
                VariableType.ListOfStrings => "AsArray().GetEnumerator().ToEnumerable().ToList()",
                VariableType.String => "ToString()",
                _ => throw new NotImplementedException() // Dictionary not implemented yet
            };
        }

        private string ToCSharpType(VariableType type)
        {
            return type switch
            {
                VariableType.Bool => "bool",
                VariableType.ByteArray => "byte[]",
                VariableType.Float => "float",
                VariableType.Int => "int",
                VariableType.ListOfStrings => "List<string>",
                VariableType.String => "string",
                _ => throw new NotImplementedException() // Dictionary not implemented yet
            };
        }

        private string GetScriptFileExtension(Interpreter interpreter)
        {
            return interpreter switch
            {
                Interpreter.Jint => "js",
                Interpreter.NodeJS => "js",
                Interpreter.IronPython => "py",
                _ => throw new NotImplementedException()
            };
        }

        private string MakeNodeObject()
            => string.Join("\r\n", OutputVariables.Select(o => $"  '{o.Name}': {o.Name},"));

        private string MakeInputs()
            => string.Join(",", InputVariables.Split(',').Select(i => SanitizeInput(i)));

        // Converts input.DATA into DATA
        private string SanitizeInput(string input)
            => Regex.Match(input, "[A-Za-z0-9_]+$").Value;
    }
}
