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
using OpenBullet2.Models.Logging;
using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
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
            // If we're in LoliCode mode, build the Stack
            if (Config.Mode == ConfigMode.LoliCode)
                Config.Stack = new Loli2StackTranspiler().Transpile(Config.LoliCodeScript);

            // Build the C# script
            Config.CSharpScript = new Stack2CSharpTranspiler().Transpile(Config.Stack);
            
            logger = new BotLogger();
            variables.Clear();
            isRunning = true;
            cts = new CancellationTokenSource();

            BotData data = new BotData(Static.RuriLibSettings, Config.Settings, logger, new Random(), null, null);

            var script = new ScriptBuilder().Build(Config);
            
            try
            {
                var state = await script.RunAsync(new ScriptGlobals(data), null, cts.Token);

                foreach (var scriptVar in state.Variables)
                {
                    var type = DescriptorsRepository.ToVariableType(scriptVar.Type);
                    
                    if (type.HasValue)
                        variables.Add(DescriptorsRepository.ToVariable(scriptVar.Name, scriptVar.Type, scriptVar.Value));
                    
                }

                logger.Log($"BOT ENDED WITH STATUS: {data.STATUS}");
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
