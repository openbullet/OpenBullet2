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
            foreach (var block in blocks)
            {
                if (block is LoliCodeBlockInstance)
                {
                    writer.Write(block.ToLC());
                }
                else
                {
                    writer.WriteLine($"BLOCK:{block.Id}");
                    writer.Write(block.ToLC());
                    writer.WriteLine("ENDBLOCK");
                    writer.WriteLine();
                }
            }
            return writer.ToString();
        }
    }
}
