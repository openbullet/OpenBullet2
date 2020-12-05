using RuriLib.Models.Blocks;
using System.Collections.Generic;
using System.IO;

namespace RuriLib.Helpers.Transpilers
{
    public class Stack2LoliTranspiler
    {
        public string Transpile(List<BlockInstance> blocks)
        {
            using var writer = new StringWriter();

            for (int i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];

                if (block is LoliCodeBlockInstance)
                {
                    writer.Write(block.ToLC());
                }
                else
                {
                    // Leave a blank line if the previous block was not a LoliCode block
                    if (i > 0 && !(blocks[i - 1] is LoliCodeBlockInstance))
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
