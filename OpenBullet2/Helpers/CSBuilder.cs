using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using OpenBullet2.Enums;
using OpenBullet2.Models;
using OpenBullet2.Models.Configs;
using OpenBullet2.Models.Settings;
using RuriLib.Models.Bots;
using RuriLib.Models.Variables;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenBullet2.Helpers
{
    public static class CSBuilder
    {
        public static void Compile(Config config)
        {
            var script = FromBlocks(config);
            config.CSharpScript = script;

            /*
            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithUsings(GetUsings(config));

            CSharpCompilation compilation = CSharpCompilation.Create(config.Id).WithOptions(options);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(script);
            */
        }

        public static string FromBlocks(Config config)
        {
            List<string> declaredVariables = typeof(BotData).GetProperties()
                .Select(p => $"data.{p.Name}").ToList();

            StringBuilder sb = new StringBuilder();

            foreach (var block in config.Blocks.Where(b => !b.Disabled))
                FromBlock(sb, block, declaredVariables);

            return sb.ToString();
        }

        private static void FromBlock(StringBuilder sb, BlockInstance block, List<string> declaredVariables)
        {
            // If not void, do variable assignment
            if (block.Info.ReturnType.HasValue)
            {
                if (declaredVariables.Contains(block.OutputVariable))
                {
                    sb.Append($"{block.OutputVariable} = ");
                }
                else
                {
                    declaredVariables.Add(block.OutputVariable);
                    sb.Append($"var {block.OutputVariable} = ");
                }
            }

            // If async, prepend the await keyword
            if (block.Info.Async)
                sb.Append("await ");

            // Append MethodName(data, param1, "param2", param3);
            var parameters = new List<string> { "data" }
                .Concat(block.Settings.Settings.Select(s => FromSetting(s)));
            
            sb.AppendLine($"{block.Info.MethodName}({string.Join(", ", parameters)});");
        }

        private static string FromSetting(BlockSetting setting)
        {
            if (setting.InputMode == InputMode.Variable)
                return setting.InputVariableName;

            if (setting.InputMode == InputMode.Interpolated)
                throw new NotImplementedException();

            return setting.FixedSetting switch
            {
                BoolSetting x => ToPrimitive(x.Value),
                ByteArraySetting x => ToPrimitive(x.Value),
                DictionaryOfStringsSetting x => ToPrimitive(x.Value),
                FloatSetting x => ToPrimitive(x.Value),
                IntSetting x => ToPrimitive(x.Value),
                ListOfStringsSetting x => ToPrimitive(x.Value),
                StringSetting x => ToPrimitive(x.Value),
                _ => throw new NotImplementedException(),
            };
        }

        private static string ToPrimitive(object value)
        {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");
            
            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, null);
            return writer.ToString();
        }

        public static string[] GetUsings()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.Contains("RuriLib"));
            var categories = assembly.GetTypes().Where(t => t.GetCustomAttribute<RuriLib.Attributes.BlockCategory>() != null);
            List<string> usings = new List<string> 
            {
                "RuriLib.Models.Bots" 
            };
            usings.AddRange(categories.Select(c => $"{c.Namespace}.{c.Name}"));
            return usings.ToArray();
        }

        class CSharpLanguage : ILanguageService
        {
            private static readonly LanguageVersion MaxLanguageVersion = Enum
                .GetValues(typeof(LanguageVersion))
                .Cast<LanguageVersion>()
                .Max();

            public SyntaxTree ParseText(string sourceCode, SourceCodeKind kind)
            {
                var options = new CSharpParseOptions(kind: kind, languageVersion: MaxLanguageVersion);

                // Return a syntax tree of our source code
                return CSharpSyntaxTree.ParseText(sourceCode, options);
            }

            public Compilation CreateLibraryCompilation(string assemblyName, bool enableOptimisations)
            {
                throw new NotImplementedException();
            }
        }
    }
}
