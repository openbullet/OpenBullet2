using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using OpenBullet2.Services;
using OpenBullet2.Shared;
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
        [Inject] public OBLogger OBLogger { get; set; }
        [Inject] ConfigService ConfigService { get; set; }

        private Config config;
        private BlockInstance selectedBlock;
        private StackViewer stackViewer;

        protected override async Task OnInitializedAsync()
        {
            config = ConfigService.SelectedConfig;
            
            try
            {
                config.ChangeMode(ConfigMode.Stack);
            }
            catch (Exception ex)
            {
                await OBLogger.LogException(ex);
                await js.AlertError(ex.GetType().Name, ex.Message);
            }

            base.OnInitialized();
        }

        private async Task SelectedBlock(BlockInstance block)
        {
            selectedBlock = block;

            if (block != null)
                await OBLogger.LogInfo($"Selected block {block.Descriptor.Id}");
        }

        private async Task AddBlock(BlockDescriptor descriptor)
        {
            selectedBlock = new BlockFactory().GetBlock<BlockInstance>(descriptor.Id);
            config.Stack.Add(selectedBlock);
            await OBLogger.LogInfo($"Added block {descriptor.Id}");
        }
    }
}
