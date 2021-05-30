using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using System;

namespace RuriLib.Helpers.Blocks
{
    /// <summary>
    /// In charge of creating new blocks.
    /// </summary>
    public class BlockFactory
    {
        /// <summary>
        /// The label of the block, useful to specify the purpose of the block.
        /// </summary>
        public string Label { get; set; } = null;

        /// <summary>
        /// Whether the block is disabled and will not be executed.
        /// </summary>
        public bool Disabled { get; set; } = false;

        /// <summary>
        /// Gets a block by <paramref name="id"/> and casts it to the requested type.
        /// </summary>
        public static T GetBlock<T>(string id) where T : BlockInstance
        {
            if (!Globals.DescriptorsRepository.Descriptors.TryGetValue(id, out BlockDescriptor descriptor))
                throw new Exception($"Invalid block id: {id}");

            BlockInstance instance = descriptor switch
            {
                AutoBlockDescriptor x => new AutoBlockInstance(x),
                KeycheckBlockDescriptor x => new KeycheckBlockInstance(x),
                HttpRequestBlockDescriptor x => new HttpRequestBlockInstance(x),
                ParseBlockDescriptor x => new ParseBlockInstance(x),
                ScriptBlockDescriptor x => new ScriptBlockInstance(x),
                _ => throw new NotImplementedException()
            };

            return instance as T;
        }
    }
}
