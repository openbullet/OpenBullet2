using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazaco.Editor;
using Blazaco.Editor.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Configs;

namespace OpenBullet2.Shared
{
    public partial class Debugger
    {
        [Parameter] public Config Config { get; set; }
        private string code = "var parsed = ParseBetweenStrings(\"how are you\", \"how\", \"you\");";

        private EditorModel _editorModel { get; set; }
        private MonacoEditor _editor;

        private List<string> log = new List<string>();
        private CancellationTokenSource cts;

        protected override void OnInitialized()
        {
            var options = new EditorOptions()
            {
                Value = code,
                Language = "csharp",
                Theme = "vs-dark",
                ReadOnly = true,
                Minimap = new MinimapOptions()
                {
                    Enabled = false
                }
            };

            _editorModel = new EditorModel(options);
            base.OnInitialized();
        }

        private void Log(string message)
        {
            log.Add(message);
        }

        private async Task Compile()
        {
            code = CSBuilder.FromBlocks(Config);
            await _editor.SetValue(code);
        }

        private async Task Run()
        {
            log = new List<string>();
            isRunning = true;
            cts = new CancellationTokenSource();

            ScriptOptions options = ScriptOptions.Default
                .AddReferences(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains("RuriLib")))
                .AddImports(CSBuilder.GetUsings(Config));

            try
            {
                var state = await CSharpScript.RunAsync<int>(code, options, null, null, cts.Token);
                foreach (var variable in state.Variables)
                    Log($"{variable.Name} = {variable.Value} of type {variable.Type}");
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().ToString(), ex.Message);
            }
            finally
            {
                isRunning = false;
            }
        }

        private void Stop()
        {
            cts.Cancel();
        }
    }
}
