using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using OpenBullet2.Enums;
using OpenBullet2.Models;
using OpenBullet2.Models.Configs;
using OpenBullet2.Models.Settings;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBullet2.Helpers
{
    public static class CSBuilder
    {
        public static void Compile(Config config)
        {
            var script = FromBlocks(config);
            config.CSharpScript = script;

            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithUsings(GetUsings(config));

            CSharpCompilation compilation = CSharpCompilation.Create(config.Id).WithOptions(options);
            SyntaxTree tree = CSharpSyntaxTree.ParseText(script);
        }

        public static string FromBlocks(Config config)
        {
            StringBuilder sb = new StringBuilder();

            // TODO: Add the fixed variable declarations here

            foreach (var block in config.Blocks)
                FromBlock(sb, block);

            return sb.ToString();
        }

        private static void FromBlock(StringBuilder sb, BlockInstance block)
        {
            // If not void, do variable assignment
            if (block.Info.ReturnType.HasValue)
                sb.Append($"var {block.OutputVariable} = ");

            // If async, prepend the await keyword
            if (block.Info.Async)
                sb.Append("await ");

            // TODO: Append CancellationToken parameter

            // Append MethodName(setting1, "setting2", setting3);
            sb.AppendLine($"{block.Info.MethodName}({string.Join(", ", block.Settings.Settings.Select(s => FromSetting(s)))});");
        }

        private static string FromSetting(BlockSetting setting)
        {
            if (setting.InputMode == InputMode.Variable)
                return setting.InputVariableName;

            if (setting.InputMode == InputMode.Interpolated)
                throw new NotImplementedException();

            switch (setting.FixedSetting)
            {
                case BoolSetting x:
                    return ToPrimitive(x.Value);

                case ByteArraySetting x:
                    return ToPrimitive(x.Value);

                case DictionaryOfStringsSetting x:
                    return ToPrimitive(x.Value);

                case FloatSetting x:
                    return ToPrimitive(x.Value);

                case IntSetting x:
                    return ToPrimitive(x.Value);

                case ListOfStringsSetting x:
                    return ToPrimitive(x.Value);

                case StringSetting x:
                    return ToPrimitive(x.Value);

                default:
                    throw new NotImplementedException();
            }
        }

        private static string ToPrimitive(object value)
        {
            using var writer = new StringWriter();
            using var provider = CodeDomProvider.CreateProvider("CSharp");
            
            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(value), writer, null);
            return writer.ToString();
        }

        public static string[] GetUsings(Config config)
        {
            return config.Blocks
                .Select(block => $"RuriLib.ExposedMethods.{block.Info.Category}.Methods")
                .Distinct().ToArray();
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
