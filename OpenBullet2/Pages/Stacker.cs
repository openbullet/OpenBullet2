using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Stacker
    {
        [Inject] ConfigService ConfigService { get; set; }

        private List<BlockDescriptor> availableBlocks;
        private Config config;
        private BlockInstance selectedBlock;

        protected override async Task OnInitializedAsync()
        {
            config = ConfigService.SelectedConfig;
            
            try
            {
                config.ChangeMode(ConfigMode.Stack);
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().ToString(), ex.Message);
            }

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
