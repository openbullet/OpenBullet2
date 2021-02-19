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

        private async Task SelectedBlock(BlockInstance block)
        {
            // If we're switching between 2 lolicode blocks, do this to force a refresh
            // of the component, otherwise the text in the editor does not update
            if (selectedBlock != null && selectedBlock is LoliCodeBlockInstance &&
                block != null && block is LoliCodeBlockInstance)
            {
                selectedBlock = null;
                StateHasChanged();
                await Task.Delay(1);
                selectedBlock = block;
            }
            else
            {
                selectedBlock = block;
            }
        }
    }
}
