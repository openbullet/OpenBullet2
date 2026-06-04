using RuriLib.Helpers.CSharp;
using RuriLib.Models.Blocks;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RuriLib.Helpers.Transpilers;

/// <summary>
/// Takes care of transpiling a list of blocks to a C# script.
/// </summary>
public class Stack2CSharpTranspiler
{
    /// <summary>
    /// Transpiles a list of <paramref name="blocks"/> to a C# script.
    /// </summary>
    /// <param name="blocks">The blocks to transpile.</param>
    /// <param name="settings">The config settings that influence generated code.</param>
    /// <param name="stepByStep">Whether step-by-step mode should be enabled.</param>
    /// <returns>The generated C# script.</returns>
    public static string Transpile(List<BlockInstance> blocks, ConfigSettings settings, bool stepByStep = false)
    {
        var declaredVariables = typeof(BotData).GetProperties()
            .Select(p => $"data.{p.Name}").ToList();
        var validBlocks = blocks.Where(b => !b.Disabled).ToList();
        using var writer = new StringWriter();
        var syntaxContext = new BlockSyntaxGenerationContext(declaredVariables, settings, stepByStep);

        foreach (var block in validBlocks)
        {
            writer.WriteLine($"// BLOCK: {block.Label}");
            writer.WriteLine($"data.ExecutingBlock({CSharpWriter.SerializeString(block.Label)});");

            if (block is LoliCodeBlockInstance loliCodeBlock)
            {
                writer.Write(loliCodeBlock.BuildScriptSnippet(syntaxContext.DefinedVariables, stepByStep));
            }
            else
            {
                writer.Write(block.ToSyntax(syntaxContext).ToSnippet());
            }

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
