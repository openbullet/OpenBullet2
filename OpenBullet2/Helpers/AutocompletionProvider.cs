using Microsoft.JSInterop;
using RuriLib;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System.Collections.Generic;

namespace OpenBullet2.Helpers
{
    public static class AutocompletionProvider
    {
        private static Dictionary<string, string> blockSnippets;

        public static void Init()
        {
            blockSnippets = new();
            foreach (var id in Globals.DescriptorsRepository.Descriptors.Keys)
            {
                var block = BlockFactory.GetBlock<BlockInstance>(id);
                blockSnippets[id] = block.ToLC(true);
            }
        }

        [JSInvokable]
        public static Dictionary<string, string> GetBlockSnippets()
            => blockSnippets;
    }
}
