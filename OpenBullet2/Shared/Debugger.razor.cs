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
using RuriLib.Models.Data.Resources;
using RuriLib.Models.Data.Resources.Options;
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
                RNG = RNGProvider
            };

            if (!RuriLibSettings.RuriLibSettings.GeneralSettings.UseCustomUserAgentsList)
            {
                providers.RandomUA = RandomUAProvider;
            }

            // Build the BotData
            var data = new BotData(providers, Config.Settings, logger, dataLine, proxy, options.UseProxy);
            data.CancellationToken = cts.Token;
            var httpClient = new HttpClient();
            data.SetObject("httpClient", httpClient);
            var runtime = Python.CreateRuntime();
            var pyengine = runtime.GetEngine("py");
            var pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
            data.SetObject("ironPyEngine", pyengine);
            data.AsyncLocker = new();

            dynamic globals = new ExpandoObject();

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

            // Initialize resources
            Dictionary<string, ConfigResource> resources = new();

            // Resources will need to be disposed of
            foreach (var opt in Config.Settings.DataSettings.Resources)
            {
                try
                {
                    resources[opt.Name] = opt switch
                    {
                        LinesFromFileResourceOptions x => new LinesFromFileResource(x),
                        RandomLinesFromFileResourceOptions x => new RandomLinesFromFileResource(x),
                        _ => throw new NotImplementedException()
                    };
                }
                catch
                {
                    logger.Log($"Could not create resource {opt.Name}", LogColors.Tomato);
                }
            }

            // Add resources to global variables
            globals.Resources = resources;
            var scriptGlobals = new ScriptGlobals(data, globals);

            // Set custom inputs
            foreach (var input in Config.Settings.InputSettings.CustomInputs)
            {
                (scriptGlobals.input as IDictionary<string, object>).Add(input.VariableName, input.DefaultAnswer);
            }

            try
            {
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
                
                var logErrorMessage = RuriLibSettings.RuriLibSettings.GeneralSettings.VerboseMode
                    ? ex.ToString()
                    : ex.Message;

                logger.Log($"[{data.ExecutionInfo}] {ex.GetType().Name}: {logErrorMessage}", LogColors.Tomato);
                await js.AlertError(ex.GetType().Name, $"[{data.ExecutionInfo}] {ex.Message}");
            }
            finally
            {
                sw.Stop();
                isRunning = false;

                logger.Log($"BOT ENDED AFTER {sw.ElapsedMilliseconds} ms WITH STATUS: {data.STATUS}");

                // Save the browser for later use
                lastBrowser = data.TryGetObject<Browser>("puppeteer");

                // Dispose stuff in data.Objects
                data.DisposeObjectsExcept(new[] { "puppeteer", "puppeteerPage", "puppeteerFrame" });

                // Dispose resources
                foreach (var resource in resources.Where(r => r.Value is IDisposable)
                    .Select(r => r.Value).Cast<IDisposable>())
                {
                    try
                    {
                        resource.Dispose();
                    }
                    catch
                    {

                    }
                }

                try
                {
                    httpClient.Dispose();
                }
                catch
                {

                }

                try
                {
                    data.AsyncLocker.Dispose();
                }
                catch
                {

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
