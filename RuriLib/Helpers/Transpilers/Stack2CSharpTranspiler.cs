using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RuriLib.Helpers.CSharp;
using RuriLib.Models.Blocks;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RuriLib.Helpers.Transpilers
{
    public class Stack2CSharpTranspiler
    {
        public static string Transpile(List<BlockInstance> blocks, ConfigSettings settings)
        {
            var declaredVariables = typeof(BotData).GetProperties()
                .Select(p => $"data.{p.Name}").ToList();

            using var writer = new StringWriter();

            foreach (var block in blocks.Where(b => !b.Disabled))
            {
                writer.WriteLine($"// BLOCK: {block.Label}");
                writer.WriteLine($"data.ExecutingBlock({CSharpWriter.SerializeString(block.Label)});");

                var snippet = block.ToCSharp(declaredVariables, settings);
                var tree = CSharpSyntaxTree.ParseText(snippet);
                writer.WriteLine(tree.GetRoot().NormalizeWhitespace().ToFullString());
                writer.WriteLine();
            }

            return writer.ToString();
        }
    }
}
