using Blazaco.Editor;
using Blazaco.Editor.Options;
using RuriLib.Models.Configs;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditLC
    {
        private EditorModel _editorModel { get; set; }
        private MonacoEditor _editor { get; set; }
        private Config config;

        protected override void OnInitialized()
        {
            config = Static.Config;
            config.ChangeMode(ConfigMode.LoliCode);

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
