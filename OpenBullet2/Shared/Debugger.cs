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
using OpenBullet2.Models.Debugger;
using OpenBullet2.Models.Logging;
using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;

namespace OpenBullet2.Shared
{
    public partial class Debugger
    {
        [Parameter] public Config Config { get; set; }

        private List<Variable> variables = new List<Variable>();
        private BotLogger logger;
        private CancellationTokenSource cts;
        private DebuggerOptions options;

        protected override void OnInitialized()
        {
            options = Static.DebuggerOptions;
            logger = Static.DebuggerLog;
        }

        private async Task Run()
        {
            try
            {
                // If we're in LoliCode mode, build the Stack
                if (Config.Mode == ConfigMode.LoliCode)
                    Config.Stack = new Loli2StackTranspiler().Transpile(Config.LoliCodeScript);

                // Build the C# script
                Config.CSharpScript = new Stack2CSharpTranspiler().Transpile(Config.Stack);
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().ToString(), ex.Message);
            }

            logger.Clear();
            variables.Clear();
            isRunning = true;
            cts = new CancellationTokenSource();

            var wordlistType = Static.Environment.WordlistTypes.First(w => w.Name == options.WordlistType);
            var dataLine = new DataLine(options.TestData, wordlistType);
            var proxy = options.UseProxy ? Proxy.Parse(options.TestProxy, options.ProxyType) : null;

            // Build the BotData
            BotData data = new BotData(Static.RuriLibSettings, Config.Settings, logger, new Random(), dataLine, proxy);

            var script = new ScriptBuilder()
                .ConfigureSlices(dataLine.GetVariables())
                .Build(Config);
            
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
