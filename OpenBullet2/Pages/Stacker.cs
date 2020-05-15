using OpenBullet2.Helpers;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Stacker
    {
        private List<BlockDescriptor> availableBlocks;
        private Config config;
        private BlockInstance selectedBlock;

        protected override void OnInitialized()
        {
            config = Static.Config;
            config.ChangeMode(ConfigMode.Stack);

            availableBlocks = RuriLib.Globals.DescriptorsRepository.Descriptors;

            base.OnInitialized();
        }

        private async Task SelectedBlock(BlockInstance block)
        {
            selectedBlock = block;

            if (block != null)
            {
                await js.Log($"Selected block {block.Descriptor.Id}");
            }
        }

        private async Task AddBlock(BlockDescriptor descriptor)
        {
            selectedBlock = new BlockFactory().GetBlock<BlockInstance>(descriptor.Id);
            config.Stack.Add(selectedBlock);
            await js.Log($"Added block {descriptor.Id}");
        }
    }
}
