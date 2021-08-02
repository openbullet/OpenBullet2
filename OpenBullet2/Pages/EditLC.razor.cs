using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Core.Services;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditLC
    {
        [Inject] private BrowserConsoleLogger OBLogger { get; set; }
        [Inject] private ConfigService ConfigService { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        private MonacoEditor _editor { get; set; }
        private Config config;

        // These solve a race condition between OnInitializedAsync and OnAfterRender that make
        // and old LoliCode get printed
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

            try
            {
                config.ChangeMode(ConfigMode.LoliCode);
            }
            catch (Exception ex)
            {
                await OBLogger.LogException(ex);
                await js.AlertException(ex);
            }
            finally
            {
                initialized = true;
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (initialized && !rendered)
            {
                _editor.SetValue(config.LoliCodeScript);
                rendered = true;
            }
        }

        private async Task OnMonacoInit()
        {
            await js.RegisterLoliCode();
            var model = await _editor.GetModel();
            await MonacoEditorBase.SetModelLanguage(model, "lolicode");
            await MonacoThemeSetter.SetLolicodeTheme(OBSettingsService.Settings.CustomizationSettings);
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
                Value = config.LoliCodeScript
            };
        }

        private async Task SaveScript()
        {
            config.LoliCodeScript = await _editor.GetValue();
        }
    }
}
