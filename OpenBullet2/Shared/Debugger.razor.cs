using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.AspNetCore.Components;
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
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;

namespace OpenBullet2.Shared
{
    public partial class Debugger
    {
        [Parameter] public Config Config { get; set; }

        [Inject] private IRandomUAProvider RandomUAProvider { get; set; }
        [Inject] private IRNGProvider RNGProvider { get; set; }
        [Inject] private RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] private PluginRepository PluginRepo { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }

        private BotLogger logger;
        private CancellationTokenSource cts;
        private DebuggerOptions options;
        private BotLoggerViewer loggerViewer;
        private VariablesViewer variablesViewer;
        private Browser lastBrowser;
        private bool showVariables = false;

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
                    Config.CSharpScript = Config.Mode == ConfigMode.Stack
                        ? Stack2CSharpTranspiler.Transpile(Config.Stack, Config.Settings)
                        : Loli2CSharpTranspiler.Transpile(Config.LoliCodeScript, Config.Settings);
                }
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }

            if (options.UseProxy && !options.TestProxy.Contains(':'))
            {
                await js.AlertError(Loc["InvalidProxy"], Loc["InvalidProxyMessage"]);
                return;
            }

            if (!options.PersistLog)
                logger.Clear();

            // Close any previously opened browser
            if (lastBrowser != null)
                await lastBrowser.CloseAsync();

            options.Variables.Clear();
            isRunning = true;
            cts = new CancellationTokenSource();
            var sw = new Stopwatch();

            var wordlistType = RuriLibSettings.Environment.WordlistTypes.First(w => w.Name == options.WordlistType);
            var dataLine = new DataLine(options.TestData, wordlistType);
            var proxy = options.UseProxy ? Proxy.Parse(options.TestProxy, options.ProxyType) : null;

            var providers = new Providers(RuriLibSettings)
            {
                RandomUA = RandomUAProvider,
                RNG = RNGProvider
            };

            // Build the BotData
            var data = new BotData(providers, Config.Settings, logger, dataLine, proxy, options.UseProxy);
            data.CancellationToken = cts.Token;
            data.Objects.Add("httpClient", new HttpClient());
            var runtime = Python.CreateRuntime();
            var pyengine = runtime.GetEngine("py");
            var pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
            data.Objects.Add("ironPyEngine", pyengine);

            var script = new ScriptBuilder()
                .Build(Config.CSharpScript, Config.Settings.ScriptSettings, PluginRepo);

            logger.Log($"Sliced {dataLine.Data} into:");
            foreach (var slice in dataLine.GetVariables())
            {
                var sliceValue = data.ConfigSettings.DataSettings.UrlEncodeDataAfterSlicing
                    ? Uri.EscapeDataString(slice.AsString())
                    : slice.AsString();

                logger.Log($"{slice.Name}: {sliceValue}");
            }

            logger.NewEntry += OnNewEntry;
            
            try
            {
                var scriptGlobals = new ScriptGlobals(data, new ExpandoObject());
                foreach (var input in Config.Settings.InputSettings.CustomInputs)
                    (scriptGlobals.input as IDictionary<string, object>).Add(input.VariableName, input.DefaultAnswer);

                sw.Start();
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
            catch (OperationCanceledException)
            {
                data.STATUS = "ERROR";
                logger.Log($"Operation canceled", LogColors.Tomato);
            }
            catch (Exception ex)
            {
                data.STATUS = "ERROR";
                logger.Log($"[{data.ExecutionInfo}] {ex.GetType().Name}: {ex.Message}", LogColors.Tomato);
                await js.AlertError(ex.GetType().Name, $"[{data.ExecutionInfo}] {ex.Message}");
            }
            finally
            {
                sw.Stop();
                isRunning = false;

                logger.Log($"BOT ENDED AFTER {sw.ElapsedMilliseconds} ms WITH STATUS: {data.STATUS}");

                // Save the browser for later use
                lastBrowser = data.Objects.ContainsKey("puppeteer") && data.Objects["puppeteer"] is Browser currentBrowser
                    ? currentBrowser
                    : null;

                // Dispose all disposable objects
                foreach (var obj in data.Objects.Where(o => o.Value is IDisposable))
                {
                    if (obj.Key == "puppeteer")
                        continue;

                    try
                    {
                        (obj.Value as IDisposable).Dispose();
                    }
                    catch
                    {

                    }
                }
            }

            await loggerViewer?.Refresh();
            variablesViewer?.Refresh();
            await InvokeAsync(StateHasChanged);
        }

        private void Stop() => cts.Cancel();

        private void OnNewEntry(object sender, BotLoggerEntry entry)
            => loggerViewer?.Refresh();

        private void ToggleView()
            => showVariables = !showVariables;
    }
}
