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

        private MonacoEditor Editor { get; set; }
        private MonacoEditor StartupEditor { get; set; }
        private Config config;

        // These solve a race condition between OnInitializedAsync and OnAfterRender that make
        // and old LoliCode get printed
        private bool initialized = false;
        private bool rendered = false;
        private bool startupEditorRendered = false;

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

                if (config.StartupLoliCodeScript is not null &&
                    config.StartupLoliCodeScript.Length > 0)
                {
                    showStartupEditor = true;
                }
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
                Editor.SetValue(config.LoliCodeScript);
                rendered = true;
            }

            if (initialized && !startupEditorRendered)
            {
                StartupEditor.SetValue(config.StartupLoliCodeScript);
                startupEditorRendered = true;
            }
        }

        private async Task OnMonacoInit()
        {
            await js.RegisterLoliCode();
            var model = await Editor.GetModel();
            await MonacoEditorBase.SetModelLanguage(model, "lolicode");
            await MonacoThemeSetter.SetLoliCodeTheme(OBSettingsService.Settings.CustomizationSettings);
        }

        private async Task OnStartupMonacoInit()
        {
            await js.RegisterLoliCode();
            var model = await StartupEditor.GetModel();
            await MonacoEditorBase.SetModelLanguage(model, "lolicode");
            await MonacoThemeSetter.SetLoliCodeTheme(OBSettingsService.Settings.CustomizationSettings);
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
                WordWrap = OBSettingsService.Settings.CustomizationSettings.WordWrap ? "on" : "off",
                Value = config.LoliCodeScript
            };
        }

        private StandaloneEditorConstructionOptions StartupEditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                Theme = OBSettingsService.Settings.CustomizationSettings.MonacoTheme,
                Language = "csharp",
                MatchBrackets = true,
                WordWrap = OBSettingsService.Settings.CustomizationSettings.WordWrap ? "on" : "off",
                Value = config.StartupLoliCodeScript
            };
        }

        private async Task SaveScript() =>
            config.LoliCodeScript = await Editor.GetValue();

        private async Task SaveStartupScript() =>
            config.StartupLoliCodeScript = await StartupEditor.GetValue();
    }
}
