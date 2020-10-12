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
        public string Transpile(List<BlockInstance> blocks, ConfigSettings settings)
        {
            List<string> declaredVariables = typeof(BotData).GetProperties()
                .Select(p => $"data.{p.Name}").ToList();

            using var writer = new StringWriter();

            foreach (var block in blocks.Where(b => !b.Disabled))
            {
                writer.WriteLine($"data.ExecutingBlock({CSharpWriter.SerializeString(block.Label)});");
                writer.Write(block.ToCSharp(declaredVariables, settings));
            }

            return writer.ToString();
        }
    }
}
