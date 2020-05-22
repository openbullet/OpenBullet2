using Blazaco.Editor;
using Blazaco.Editor.Options;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class EditCS
    {
        [Inject] NavigationManager Nav { get; set; }
        [Inject] ConfigService ConfigService { get; set; }

        [Parameter] public Config Config { get; set; }
        private EditorModel _editorModel { get; set; }
        private MonacoEditor _editor { get; set; }

        protected override void OnInitialized()
        {
            Config = ConfigService.SelectedConfig;

            var options = new EditorOptions()
            {
                Value = Config.CSharpScript,
                Language = "csharp",
                Theme = "vs-dark",
                ReadOnly = Config.Mode != ConfigMode.CSharp,
                Minimap = new MinimapOptions()
                {
                    Enabled = false
                }
            };

            _editorModel = new EditorModel(options);
            base.OnInitialized();
        }

        private async Task Compile()
        {
            var stack = new Loli2StackTranspiler().Transpile(Config.LoliCodeScript);
            Config.CSharpScript = new Stack2CSharpTranspiler().Transpile(stack);
            await _editor.SetValue(Config.CSharpScript);
        }

        private async Task ConvertConfig()
        {
            var confirmed = await js.Confirm("WARNING! PLEASE READ!", 
                "Once you convert the config to C# only, you won't be able to edit it with stacker anymore! Are you really, REALLY sure you know what you're doing?");
            
            if (!confirmed)
                return;

            Config.ChangeMode(ConfigMode.CSharp);
            ConfigService.SelectedConfig = Config;
            Nav.NavigateTo("config/edit/code", true);
        }
    }
}
