using System;
using System.Collections.Generic;

namespace RuriLib.Legacy.Blocks
{
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

        /// <inheritdoc />
        public override BlockBase FromLS(List<string> lines)
        {
            Script = string.Join(Environment.NewLine, lines);
            return this;
        }

        /// <inheritdoc />
        public override string ToLS(bool indent = true) => Script;
    }
}
