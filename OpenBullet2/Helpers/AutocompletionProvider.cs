using Microsoft.JSInterop;
using OpenBullet2.Core.Models.Settings;
using RuriLib;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System.Collections.Generic;

namespace OpenBullet2.Helpers
{
    public static class AutocompletionProvider
    {
        private static Dictionary<string, string> blockSnippets = new();
        private static Dictionary<string, string> customSnippets = new();

        public static void Init(List<CustomSnippet> custom)
        {
            foreach (var id in Globals.DescriptorsRepository.Descriptors.Keys)
            {
                var block = BlockFactory.GetBlock<BlockInstance>(id);
                blockSnippets[id] = block.ToLC(true);
            }

            foreach (var snippet in custom)
            {
                if (!string.IsNullOrEmpty(snippet.Name))
                {
                    customSnippets[snippet.Name] = snippet.Body;
                }
            }
        }

        [JSInvokable]
        public static Dictionary<string, string> GetBlockSnippets()
            => blockSnippets;

        [JSInvokable]
        public static Dictionary<string, string> GetCustomSnippets()
            => customSnippets;
    }
}
