using Microsoft.JSInterop;
using RuriLib;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;

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
                blockSnippets[id] = block.ToLC();
            }
        }

        public static void Refresh()
        {
            foreach (var id in Globals.DescriptorsRepository.Descriptors.Keys.Where(id => !blockSnippets.ContainsKey(id)))
            {
                var block = BlockFactory.GetBlock<BlockInstance>(id);
                blockSnippets[id] = block.ToLC();
            }
        }

        [JSInvokable]
        public static Dictionary<string, string> GetBlockSnippets()
            => blockSnippets;
    }
}
