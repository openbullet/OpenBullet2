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
    /// <summary>
    /// Takes care of transpiling a list of blocks to a C# script.
    /// </summary>
    public class Stack2CSharpTranspiler
    {
        /// <summary>
        /// Transpiles a list of <paramref name="blocks"/> to a C# script. If <paramref name="pauseToken"/> is
        /// not null, step-by-step mode will be enabled.
        /// </summary>
        public static string Transpile(List<BlockInstance> blocks, ConfigSettings settings, bool stepByStep = false)
        {
            var declaredVariables = typeof(BotData).GetProperties()
                .Select(p => $"data.{p.Name}").ToList();

            using var writer = new StringWriter();

            var validBlocks = blocks.Where(b => !b.Disabled);

            foreach (var block in validBlocks)
            {
                writer.WriteLine($"// BLOCK: {block.Label}");
                writer.WriteLine($"data.ExecutingBlock({CSharpWriter.SerializeString(block.Label)});");

                var snippet = block.ToCSharp(declaredVariables, settings);
                var tree = CSharpSyntaxTree.ParseText(snippet);
                writer.WriteLine(tree.GetRoot().NormalizeWhitespace().ToFullString());
                writer.WriteLine();

                // If in step by step mode, and if not the last block, check if pause was requested
                if (stepByStep && block != validBlocks.Last())
                {
                    writer.WriteLine("await data.Stepper.WaitForStepAsync(data.CancellationToken);");
                }
            }

            return writer.ToString();
        }
    }
}
