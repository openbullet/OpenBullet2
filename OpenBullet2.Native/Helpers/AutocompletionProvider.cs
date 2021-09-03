using RuriLib;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System.Collections.Generic;

namespace OpenBullet2.Native.Helpers
{
    public static class AutocompletionProvider
    {
        private static List<BlockSnippet> blockSnippets;

        public static void Init()
        {
            blockSnippets = new();
            foreach (var id in Globals.DescriptorsRepository.Descriptors.Keys)
            {
                var block = BlockFactory.GetBlock<BlockInstance>(id);
                blockSnippets.Add(new BlockSnippet(id, block.ToLC(true), block.Descriptor.Description));
            }
        }

        public static List<BlockSnippet> GetBlockSnippets()
            => blockSnippets;
    }

    public struct BlockSnippet
    {
        public string Id { get; set; }
        public string Snippet { get; set; }
        public string Description { get; set; }

        public BlockSnippet(string id, string snippet, string description)
        {
            Id = id;
            Snippet = snippet;
            Description = description;
        }
    }
}
