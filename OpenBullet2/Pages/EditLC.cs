using Blazaco.Editor;
using Blazaco.Editor.Options;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using RuriLib.Models.Configs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditLC : IDisposable
    {
        [Inject] ConfigService ConfigService { get; set; }

        private EditorModel _editorModel { get; set; }
        private MonacoEditor _editor { get; set; }
        private Config config;
        private Timer timer;

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

            var options = new EditorOptions()
            {
                Value = config.LoliCodeScript,
                Language = "lolicode",
                Theme = "vs-dark",
                Minimap = new MinimapOptions()
                {
                    Enabled = false
                }
            };

            _editorModel = new EditorModel(options);
            base.OnInitialized();

            // Save the content of the editor automatically every second
            timer = new Timer(new TimerCallback(async _ =>
            {
                await InvokeAsync(SaveLoliCode);
            }), null, 1000, 1000);
        }

        private async Task SaveLoliCode()
        {
            config.LoliCodeScript = await _editor.GetValue();
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
