using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Core.Services;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using RuriLib.Models.Configs;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditLS
    {
        [Inject] private ConfigService ConfigService { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        private MonacoEditor Editor { get; set; }
        private Config config;

        // These solve a race condition between OnInitializedAsync and OnAfterRender that make
        // and old LoliScript get printed
        private bool initialized = false;
        private bool rendered = false;

        protected override async Task OnInitializedAsync()
        {
            config = ConfigService.SelectedConfig;

            if (config == null)
            {
                Nav.NavigateTo("/configs");
                return;
            }

            if (config.Mode is not ConfigMode.Legacy)
            {
                await js.AlertError(Loc["InvalidMode"], Loc["NotALegacyConfig"]);
            }

            initialized = true;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (initialized && !rendered)
            {
                Editor.SetValue(config.LoliScript);
                rendered = true;
            }
        }

        private async Task OnMonacoInit()
        {
            await js.RegisterLoliScript();
            var model = await Editor.GetModel();
            await MonacoEditorBase.SetModelLanguage(model, "loliscript");
            await MonacoThemeSetter.SetLoliScriptTheme(OBSettingsService.Settings.CustomizationSettings);
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                Theme = OBSettingsService.Settings.CustomizationSettings.MonacoTheme,
                Language = "csharp",
                MatchBrackets = true,
                Value = config.LoliScript
            };
        }

        private async Task SaveScript()
        {
            config.LoliScript = await Editor.GetValue();
        }
    }
}
