using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using OpenBullet2.Services;
using OpenBullet2.Shared;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Stacker
    {
        [Inject] private BrowserConsoleLogger OBLogger { get; set; }
        [Inject] private ConfigService ConfigService { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        private Config config;
        private BlockInstance selectedBlock;
        private StackViewer stackViewer;

        protected override async Task OnInitializedAsync()
        {
            config = ConfigService.SelectedConfig;

            if (config == null)
            {
                Nav.NavigateTo("/configs");
                return;
            }

            try
            {
                config.ChangeMode(ConfigMode.Stack);
            }
            catch (Exception ex)
            {
                await OBLogger.LogException(ex);
                await js.AlertError(ex.GetType().Name, ex.Message);
                Nav.NavigateTo("config/edit/lolicode");
            }

            base.OnInitialized();
        }

        private void SelectedBlock(BlockInstance block)
        {
            selectedBlock = block;
        }

        private async Task AddBlock(BlockDescriptor descriptor)
        {
            selectedBlock = BlockFactory.GetBlock<BlockInstance>(descriptor.Id);
            config.Stack.Add(selectedBlock);
            await OBLogger.LogInfo($"Added block {descriptor.Id}");
        }
    }
}
