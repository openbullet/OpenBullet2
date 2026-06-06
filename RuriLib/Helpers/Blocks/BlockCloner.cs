using System;
using RuriLib.Models.Blocks;

namespace RuriLib.Helpers.Blocks;

/// <summary>
/// Clones configured block instances without serializing descriptor metadata.
/// </summary>
public static class BlockCloner
{
    /// <summary>
    /// Clones a block by recreating its instance and parsing the configured LoliCode.
    /// </summary>
    public static T Clone<T>(T block) where T : BlockInstance
    {
        ArgumentNullException.ThrowIfNull(block);

        if (block is LoliCodeBlockInstance loliCodeBlock)
        {
            return (T)(BlockInstance)new LoliCodeBlockInstance(new LoliCodeBlockDescriptor())
            {
                Disabled = loliCodeBlock.Disabled,
                Label = loliCodeBlock.Label,
                Script = loliCodeBlock.Script
            };
        }

        var clonedBlock = BlockFactory.GetBlock<BlockInstance>(block.Id);
        var blockOptions = block.ToLC(printDefaultParams: true);
        var lineNumber = 0;

        clonedBlock.FromLC(ref blockOptions, ref lineNumber);

        return (T)clonedBlock;
    }
}
