using OpenBullet2.Core.Services;
using RuriLib;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System.Collections.Generic;

namespace OpenBullet2.Native.Helpers
{
    public static class AutocompletionProvider
    {
        private static List<Snippet> snippets = new();

        public static void Init()
        {
            // Block snippets
            foreach (var id in Globals.DescriptorsRepository.Descriptors.Keys)
            {
                var block = BlockFactory.GetBlock<BlockInstance>(id);
                snippets.Add(new Snippet($"BLOCK:{id}", $"BLOCK:{id}\r\n{block.ToLC(true)}ENDBLOCK", block.Descriptor.Description));
            }

            // Custom snippets
            foreach (var snippet in SP.GetService<OpenBulletSettingsService>().Settings.GeneralSettings.CustomSnippets)
            {
                if (!string.IsNullOrEmpty(snippet.Name))
                {
                    snippets.Add(new Snippet(snippet.Name, snippet.Body, snippet.Description));
                }
            }
        }

        public static List<Snippet> GetSnippets()
            => snippets;
    }

    public struct Snippet
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public string Description { get; set; }

        public Snippet(string id, string body, string description)
        {
            Id = id;
            Body = body;
            Description = description;
        }
    }
}
