using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Debugger;
using OpenBullet2.Services;
using PuppeteerSharp;
using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Transpilers;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Proxies;
using RuriLib.Models.UserAgents;
using RuriLib.Models.Variables;
using RuriLib.Services;

namespace OpenBullet2.Shared
{
    public partial class Debugger
    {
        [Inject] IModalService Modal { get; set; }
        [Inject] IRandomUAProvider RandomUAProvider { get; set; }
        [Inject] RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] VolatileSettingsService VolatileSettings { get; set; }

        [Parameter] public Config Config { get; set; }

        private BotLogger logger;
        private CancellationTokenSource cts;
        private DebuggerOptions options;
        private BotLoggerViewer loggerViewer;
        private Browser lastBrowser;

        protected override void OnInitialized()
        {
            options = VolatileSettings.DebuggerOptions;
            logger = VolatileSettings.DebuggerLog;
        }

        private async Task Run()
        {
            try
            {
                // Build the C# script if not in CSharp mode
                if (Config.Mode != ConfigMode.CSharp)
                {
                    // If we're in LoliCode mode, build the Stack
                    if (Config.Mode == ConfigMode.LoliCode)
                        Config.Stack = new Loli2StackTranspiler().Transpile(Config.LoliCodeScript);

                    // Transpile the Stack to C#
                    Config.CSharpScript = new Stack2CSharpTranspiler().Transpile(Config.Stack, Config.Settings);
                }
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }

            if (!options.PersistLog)
                logger.Clear();

            // Close any previously opened browser
            if (lastBrowser != null)
                await lastBrowser.CloseAsync();

            options.Variables.Clear();
            isRunning = true;
            cts = new CancellationTokenSource();

            var wordlistType = RuriLibSettings.Environment.WordlistTypes.First(w => w.Name == options.WordlistType);
            var dataLine = new DataLine(options.TestData, wordlistType);
            var proxy = options.UseProxy ? Proxy.Parse(options.TestProxy, options.ProxyType) : null;

            // Build the BotData
            BotData data = new BotData(RuriLibSettings.RuriLibSettings, Config.Settings, logger, RandomUAProvider, new Random(), dataLine, proxy, options.UseProxy);
            data.Objects.Add("httpClient", new HttpClient());
            var runtime = Python.CreateRuntime();
            var pyengine = runtime.GetEngine("py");
            PythonCompilerOptions pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
            data.Objects.Add("ironPyEngine", pyengine);

            var script = new ScriptBuilder()
                .Build(Config.CSharpScript, Config.Settings.ScriptSettings);

            logger.Log($"Sliced {dataLine.Data} into:");
            foreach (var slice in dataLine.GetVariables())
                logger.Log($"{slice.Name}: {slice.AsString()}");

            logger.NewEntry += OnNewEntry;
            
            try
            {
                var scriptGlobals = new ScriptGlobals(data);
                foreach (var input in Config.Settings.InputSettings.CustomInputs)
                    (scriptGlobals.input as IDictionary<string, object>).Add(input.VariableName, input.DefaultAnswer);

                var state = await script.RunAsync(scriptGlobals, null, cts.Token);

                foreach (var scriptVar in state.Variables)
                {
                    try
                    {
                        var type = DescriptorsRepository.ToVariableType(scriptVar.Type);

                        if (type.HasValue && !scriptVar.Name.StartsWith("tmp_"))
                        {
                            var variable = DescriptorsRepository.ToVariable(scriptVar.Name, scriptVar.Type, scriptVar.Value);
                            variable.MarkedForCapture = data.MarkedForCapture.Contains(scriptVar.Name);
                            options.Variables.Add(variable);
                        }
                    }
                    catch
                    {
                        // The type is not supported, e.g. it was generated using custom C# code and not blocks
                        // so we just disregard it
                    }
                }
            }
            catch (Exception ex)
            {
                data.STATUS = "ERROR";
                await js.AlertException(ex);
            }
            finally
            {
                isRunning = false;

                logger.Log($"BOT ENDED WITH STATUS: {data.STATUS}");

                // Save the browser for later use
                lastBrowser = data.Objects.ContainsKey("puppeteer") && data.Objects["puppeteer"] is Browser currentBrowser
                    ? currentBrowser
                    : null;

                // Dispose the default HttpClient if any
                if (data.Objects["httpClient"] is HttpClient client)
                    client.Dispose();
            }

            loggerViewer.Refresh();
            await InvokeAsync(StateHasChanged);
            await js.InvokeVoidAsync("adjustTextAreas").ConfigureAwait(false);
        }

        private void Stop()
        {
            cts.Cancel();
        }

        private void OnNewEntry(object sender, BotLoggerEntry entry)
            => loggerViewer?.Refresh();
    }
}
