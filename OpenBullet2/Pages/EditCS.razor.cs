using BlazorMonaco;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Core.Services;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditCS
    {
        [Parameter] public Config Config { get; set; }

        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private BrowserConsoleLogger OBLogger { get; set; }
        [Inject] private ConfigService ConfigService { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }

        private MonacoEditor Editor { get; set; }
        private MonacoEditor StartupEditor { get; set; }

        // These solve a race condition between OnInitializedAsync and OnAfterRender that make
        // and old C# code get printed
        private bool initialized = false;
        private bool rendered = false;
        private bool startupEditorRendered = false;

        protected override async Task OnInitializedAsync()
        {
            Config = ConfigService.SelectedConfig;

            if (Config == null)
            {
                Nav.NavigateTo("/configs");
                return;
            }

            // Transpile if not in CSharp mode
            if (Config != null && Config.Mode != ConfigMode.CSharp)
            {
                try
                {
                    Config.CSharpScript = Config.Mode == ConfigMode.Stack
                        ? Stack2CSharpTranspiler.Transpile(Config.Stack, Config.Settings)
                        : Loli2CSharpTranspiler.Transpile(Config.LoliCodeScript, Config.Settings);

                    Config.StartupCSharpScript = Loli2CSharpTranspiler.Transpile(
                        Config.StartupLoliCodeScript, Config.Settings);

                    if (Config.StartupCSharpScript is not null &&
                        Config.StartupLoliCodeScript.Length > 0)
                    {
                        showStartupEditor = true;
                    }
                }
                catch (Exception ex)
                {
                    await OBLogger.LogException(ex);
                }
                finally
                {
                    initialized = true;
                }
            }
        }

        private StandaloneEditorConstructionOptions EditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                ReadOnly = Config.Mode != ConfigMode.CSharp,
                Theme = OBSettingsService.Settings.CustomizationSettings.MonacoTheme,
                Language = "csharp",
                MatchBrackets = true,
                WordWrap = OBSettingsService.Settings.CustomizationSettings.WordWrap ? "on" : "off",
                Value = Config.CSharpScript
            };
        }

        private StandaloneEditorConstructionOptions StartupEditorConstructionOptions(MonacoEditor editor)
        {
            return new StandaloneEditorConstructionOptions
            {
                AutomaticLayout = true,
                Minimap = new EditorMinimapOptions { Enabled = false },
                ReadOnly = Config.Mode != ConfigMode.CSharp,
                Theme = OBSettingsService.Settings.CustomizationSettings.MonacoTheme,
                Language = "csharp",
                MatchBrackets = true,
                WordWrap = OBSettingsService.Settings.CustomizationSettings.WordWrap ? "on" : "off",
                Value = Config.StartupCSharpScript
            };
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (initialized && !rendered)
            {
                Editor.SetValue(Config.CSharpScript);
                rendered = true;
            }

            if (initialized && !startupEditorRendered)
            {
                StartupEditor.SetValue(Config.StartupCSharpScript);
                startupEditorRendered = true;
            }
        }

        private async Task ConvertConfig()
        {
            var confirmed = await js.Confirm(Loc["WarningPleaseRead"], Loc["ConfirmConfigConversion"], Loc["Cancel"]);
            
            if (!confirmed)
                return;

            Config.ChangeMode(ConfigMode.CSharp);
            ConfigService.SelectedConfig = Config;
            Nav.NavigateTo("config/edit/code", true);
        }

        private async Task SaveScript() =>
            Config.CSharpScript = await Editor.GetValue();

        private async Task SaveStartupScript() =>
            Config.StartupCSharpScript = await StartupEditor.GetValue();
    }
}
