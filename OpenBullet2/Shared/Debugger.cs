using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.JSInterop;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Configs;
using OpenBullet2.Models.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Variables;

namespace OpenBullet2.Shared
{
    public partial class Debugger
    {
        [Parameter] public Config Config { get; set; }

        private List<Variable> variables = new List<Variable>();
        private BotLogger logger = new BotLogger();
        private CancellationTokenSource cts;

        private async Task Run()
        {
            // Check if variables are ok
            /*
            try 
            {
                ConfigBuilder.CheckVariables(Config);
            }
            catch (Exception ex) 
            {
                await js.AlertError("Uh-oh!", ex.Message);
                return;
            }
            */

            // Compile the config
            CSBuilder.Compile(Config);

            logger = new BotLogger();
            variables.Clear();
            isRunning = true;
            cts = new CancellationTokenSource();

            BotData data = new BotData(Static.RuriLibSettings, Config.Settings, logger, new Random(), null, null);

            var ruriLib = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Contains("RuriLib"));
            var references = ruriLib.GetReferencedAssemblies();

            var script =
                CSharpScript.Create(
                    code: Config.CSharpScript,
                    options: ScriptOptions.Default
                        .WithReferences(ruriLib)
                        .WithImports(CSBuilder.GetUsings()),
                    globalsType: typeof(ScriptGlobals));

            script.Options.AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => references.Any(r => r.FullName == a.FullName)));

            try
            {
                var state = await script.RunAsync(new ScriptGlobals(data), null, cts.Token);

                foreach (var scriptVar in state.Variables)
                {
                    var type = BlockBuilder.ToVariableType(scriptVar.Type);
                    
                    if (type.HasValue)
                        variables.Add(BlockBuilder.ToVariable(scriptVar.Name, scriptVar.Type, scriptVar.Value));
                    
                }
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().ToString(), ex.Message);
            }
            finally
            {
                isRunning = false;
            }

            await InvokeAsync(StateHasChanged);
            await js.InvokeVoidAsync("adjustTextAreas").ConfigureAwait(false);
        }

        private void Stop()
        {
            cts.Cancel();
        }
    }
}
