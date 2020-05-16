using Blazaco.Editor;
using Blazaco.Editor.Options;
using OpenBullet2.Helpers;
using RuriLib.Models.Configs;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditLC
    {
        private EditorModel _editorModel { get; set; }
        private MonacoEditor _editor { get; set; }
        private Config config;

        protected override async Task OnInitializedAsync()
        {
            config = Static.Config;

            try
            {
                config.ChangeMode(ConfigMode.LoliCode);
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().ToString(), ex.Message);
            }

            var options = new EditorOptions()
            {
                Value = config.LoliCodeScript,
                Language = "csharp",
                Theme = "vs-dark",
                Minimap = new MinimapOptions()
                {
                    Enabled = false
                }
            };

            _editorModel = new EditorModel(options);
            base.OnInitialized();
        }

        private async Task SaveLoliCode()
        {
            config.LoliCodeScript = await _editor.GetValue();
        }
    }
}
