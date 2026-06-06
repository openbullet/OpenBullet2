using System;
using System.Collections.Generic;

namespace RuriLib.Legacy.Blocks;

/// <summary>
/// A block that contains LoliScript code for readonly visualization purposes.
/// </summary>
public class BlockLSCode : BlockBase
{
    /// <summary>The LoliScript code to display.</summary>
    public string Script { get; set; } = "";

    /// <summary>
    /// Creates a LSCode block.
    /// </summary>
    public BlockLSCode()
    {
        Label = "LS";
    }

    /// <summary>
    /// Populates the block from raw LoliScript lines.
    /// </summary>
    /// <param name="lines">The lines that belong to this block.</param>
    /// <returns>The current block instance.</returns>
    public override BlockBase FromLS(List<string> lines)
    {
        Script = string.Join(Environment.NewLine, lines);
        return this;
    }

    /// <summary>
    /// Serializes the stored raw script.
    /// </summary>
    /// <param name="indent">Unused for this block.</param>
    /// <returns>The stored script.</returns>
    public override string ToLS(bool indent = true) => Script;
}
