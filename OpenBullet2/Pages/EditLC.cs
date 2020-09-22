using BlazorMonaco;
using BlazorMonaco.Bridge;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using RuriLib.Models.Configs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditLC
    {
        [Inject] ConfigService ConfigService { get; set; }

        private MonacoEditor _editor { get; set; }
        private Config config;

        protected override async Task OnInitializedAsync()
        {
            config = ConfigService.SelectedConfig;

            try
            {
                config.ChangeMode(ConfigMode.LoliCode);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }

            base.OnInitialized();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                _editor.SetValue(config.LoliCodeScript);
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new MinimapOptions { Enabled = false },
                Theme = "vs-dark",
                Language = "lolicode",
                MatchBrackets = true,
                Value = config.LoliCodeScript
            };
        }

        private async Task SaveScript()
        {
            config.LoliCodeScript = await _editor.GetValue();
        }
    }
}
