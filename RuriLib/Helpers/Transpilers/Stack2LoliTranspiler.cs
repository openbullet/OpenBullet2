using RuriLib.Models.Blocks;
using System.Collections.Generic;
using System.IO;

namespace RuriLib.Helpers.Transpilers
{
    /// <summary>
    /// Takes care of transpiling a list of blocks to a LoliCode script.
    /// </summary>
    public static class Stack2LoliTranspiler
    {
        /// <summary>
        /// Transpiles a list of <paramref name="blocks"/> to a LoliCode script.
        /// </summary>
        public static string Transpile(List<BlockInstance> blocks)
        {
            using var writer = new StringWriter();

            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];

                if (block is LoliCodeBlockInstance)
                {
                    var loliCode = block.ToLC();
                    if (loliCode.EndsWith("\r\n") || loliCode.EndsWith("\n"))
                    {
                        writer.Write(loliCode);
                    }
                    else
                    {
                        writer.WriteLine(loliCode);
                    }
                }
                else
                {
                    // Leave a blank line if the previous block was not a LoliCode block
                    if (i > 0 && blocks[i - 1] is not LoliCodeBlockInstance)
                        writer.WriteLine();

                    writer.WriteLine($"BLOCK:{block.Id}");
                    writer.Write(block.ToLC());
                    writer.WriteLine("ENDBLOCK");
                }
            }
            return writer.ToString();
        }
    }
}
