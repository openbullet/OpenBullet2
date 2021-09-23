using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib
{
    /// <summary>
    /// A block that contains LoliScript code for readonly visualization purposes.
    /// </summary>
    public class BlockLSCode : BlockBase
    {
        private string script = "";
        /// <summary>The LoliScript code to display.</summary>
        public string Script { get { return script; } set { script = value; OnPropertyChanged(); } }

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
        public override string ToLS(bool indent = true)
        {
            return script;
        }
    }
}
