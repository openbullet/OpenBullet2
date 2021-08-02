using Microsoft.AspNetCore.Components;
using OpenBullet2.Core.Services;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using OpenBullet2.Shared;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
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
            // If we're switching between 2 blocks that have a Monaco Editor, do this to force a refresh
            // of the component, otherwise the text in the editor does not update
            if (selectedBlock != null && HasMonacoEditor(selectedBlock) &&
                block != null && HasMonacoEditor(block))
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

        private bool HasMonacoEditor(BlockInstance block)
            => block is LoliCodeBlockInstance or ScriptBlockInstance;
    }
}
