using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.CodeAnalysis.Scripting;
using PuppeteerSharp;
using RuriLib.Exceptions;
using RuriLib.Helpers;
using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Transpilers;
using RuriLib.Legacy.LS;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using RuriLib.Blocks.Interop;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Data.Resources;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Debugger;

/// <summary>
/// Represents the current execution state of a <see cref="ConfigDebugger"/>.
/// </summary>
public enum ConfigDebuggerStatus
{
    /// <summary>
    /// The debugger is idle.
    /// </summary>
    Idle,

    /// <summary>
    /// The debugger is currently running.
    /// </summary>
    Running,

    /// <summary>
    /// The debugger is waiting for an explicit step command.
    /// </summary>
    WaitingForStep
}

/// <summary>
/// Executes a config against a single test input for interactive debugging.
/// </summary>
public class ConfigDebugger : IDisposable
{
    /// <summary>
    /// The random user agent provider to use when the environment does not force a custom list.
    /// </summary>
    public IRandomUAProvider? RandomUAProvider { get; set; }

    /// <summary>
    /// The random number provider used by the debug session.
    /// </summary>
    public IRNGProvider? RNGProvider { get; set; }

    /// <summary>
    /// The settings service providing environment and library settings.
    /// </summary>
    public RuriLibSettingsService? RuriLibSettings { get; set; }

    /// <summary>
    /// The plugin repository used to build scripts.
    /// </summary>
    public PluginRepository? PluginRepo { get; set; }

    /// <summary>
    /// The current debugger status.
    /// </summary>
    public ConfigDebuggerStatus Status { get; private set; }

    /// <summary>
    /// The config being debugged.
    /// </summary>
    public Config Config { get; init; }

    /// <summary>
    /// The options controlling the debug session.
    /// </summary>
    public DebuggerOptions Options { get; init; }

    /// <summary>
    /// The logger used during execution.
    /// </summary>
    public BotLogger Logger { get; init; }

    /// <summary>
    /// Raised when the debugger status changes.
    /// </summary>
    public event EventHandler<ConfigDebuggerStatus>? StatusChanged;

    /// <summary>
    /// Raised when a new log entry is emitted.
    /// </summary>
    public event EventHandler<BotLoggerEntry>? NewLogEntry;

    /// <summary>
    /// Raised when the debugger variables snapshot changes.
    /// </summary>
    public event EventHandler? VariablesChanged;

    private BotData? data;
    private Stepper? stepper;
    private CancellationTokenSource? cts;
    private Browser? lastPuppeteerBrowser;
    private Microsoft.Playwright.IPlaywright? lastPlaywright;
    private Microsoft.Playwright.IBrowser? lastPlaywrightBrowser;
    private OpenQA.Selenium.WebDriver? lastSeleniumBrowser;

    /// <summary>
    /// Creates a new config debugger instance.
    /// </summary>
    /// <param name="config">The config to debug.</param>
    /// <param name="options">Optional debugger options.</param>
    /// <param name="logger">Optional logger to reuse.</param>
    public ConfigDebugger(Config config, DebuggerOptions? options = null, BotLogger? logger = null)
    {
        Config = config;
        Options = options ?? new DebuggerOptions();
        Logger = logger ?? new BotLogger();
        Logger.NewEntry += OnNewEntry;
    }

    /// <summary>
    /// Runs the config once against the configured test input.
    /// </summary>
    /// <returns>A task that completes when the debug session ends.</returns>
    public async Task Run()
    {
        if (RuriLibSettings is null)
        {
            throw new InvalidOperationException($"{nameof(RuriLibSettings)} must be set before running the debugger.");
        }

        if (RNGProvider is null)
        {
            throw new InvalidOperationException($"{nameof(RNGProvider)} must be set before running the debugger.");
        }

        if (PluginRepo is null)
        {
            throw new InvalidOperationException($"{nameof(PluginRepo)} must be set before running the debugger.");
        }

        // Build the C# script if in Stack or LoliCode mode
        if (Config.Mode == ConfigMode.Stack || Config.Mode == ConfigMode.LoliCode)
        {
            Config.CSharpScript = Config.Mode == ConfigMode.Stack
                ? Stack2CSharpTranspiler.Transpile(Config.Stack, Config.Settings, Options.StepByStep)
                : Loli2CSharpTranspiler.Transpile(Config.LoliCodeScript, Config.Settings, Options.StepByStep);

            // Stacker is not currently available for the startup phase
            Config.StartupCSharpScript = Loli2CSharpTranspiler.Transpile(Config.StartupLoliCodeScript, Config.Settings, Options.StepByStep);
        }

        if (Options.UseProxy && !Options.TestProxy.Contains(':'))
        {
            throw new InvalidProxyException(Options.TestProxy);
        }

        if (!Options.PersistLog)
        {
            Logger.Clear();
        }

        // Close any previously opened browsers
        if (lastPuppeteerBrowser != null)
        {
            await lastPuppeteerBrowser.CloseAsync().ConfigureAwait(false);
            await lastPuppeteerBrowser.DisposeAsync();
        }

        if (lastPlaywrightBrowser != null)
        {
            await lastPlaywrightBrowser.CloseAsync().ConfigureAwait(false);
            lastPlaywright?.Dispose();
        }

        if (lastSeleniumBrowser != null)
        {
            lastSeleniumBrowser.Quit();
            lastSeleniumBrowser.Dispose();
        }

        SetVariables([]);
        cts = new CancellationTokenSource();
        var sw = new Stopwatch();
        Dictionary<string, ConfigResource> resources = new();

        try
        {
            var wordlistType = RuriLibSettings.Environment.WordlistTypes.First(w => w.Name == Options.WordlistType);
            var dataLine = new DataLine(Options.TestData, wordlistType);
            LogInputValidationWarnings(dataLine);
            var proxy = Options.UseProxy ? Proxy.Parse(Options.TestProxy, Options.ProxyType) : null;

            var providers = new Bots.Providers(RuriLibSettings)
            {
                RNG = RNGProvider
            };

            if (!RuriLibSettings.RuriLibSettings.GeneralSettings.UseCustomUserAgentsList
                && RandomUAProvider is not null)
            {
                providers.RandomUA = RandomUAProvider;
            }

            // Unregister the previous event if there was an existing stepper
            if (stepper != null)
            {
                stepper.WaitingForStep -= OnWaitingForStep;
            }

            stepper = new Stepper();
            stepper.WaitingForStep += OnWaitingForStep;

            // Build the BotData
            data = new BotData(providers, Config.Settings, Logger, dataLine, proxy, Options.UseProxy)
            {
                CancellationToken = cts.Token,
                Stepper = stepper
            };
            using var httpClient = new HttpClient();
            data.SetObject("httpClient", httpClient);
            var runtime = Python.CreateRuntime();
            var pyengine = runtime.GetEngine("py");
            var pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
            data.SetObject("ironPyEngine", pyengine);
            data.SetObject("pythonRuntime", PythonScriptRuntime.GetShared(), false);
            data.AsyncLocker = new();

            dynamic globals = new ExpandoObject();

            var script = new ScriptBuilder()
                .Build(Config.CSharpScript, Config.Settings.ScriptSettings, PluginRepo);

            var startupScript = new ScriptBuilder().Build(Config.StartupCSharpScript, Config.Settings.ScriptSettings, PluginRepo);

            Logger.Log($"Sliced {dataLine.Data} into:");
            foreach (var slice in dataLine.GetVariables())
            {
                var sliceValue = data.ConfigSettings.DataSettings.UrlEncodeDataAfterSlicing
                    ? Uri.EscapeDataString(slice.AsString())
                    : slice.AsString();

                Logger.Log($"{slice.Name}: {sliceValue}");
            }

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
                    Logger.Log($"Could not create resource {opt.Name}", LogColors.Tomato);
                }
            }

            // Add resources to global variables
            globals.Resources = resources;
            globals.OwnerId = 0;
            globals.JobId = 0;
            var scriptGlobals = new ScriptGlobals(data, globals);

            // Set custom inputs
            foreach (var input in Config.Settings.InputSettings.CustomInputs)
            {
                (scriptGlobals.input as IDictionary<string, object?>)!.Add(input.VariableName, input.DefaultAnswer);
            }

            // [LEGACY] Set up the VariablesList
            if (Config.Mode == ConfigMode.Legacy)
            {
                var slices = new List<Variable>();

                foreach (var slice in dataLine.GetVariables())
                {
                    var sliceValue = data.ConfigSettings.DataSettings.UrlEncodeDataAfterSlicing
                        ? Uri.EscapeDataString(slice.AsString())
                        : slice.AsString();

                    slices.Add(new StringVariable(sliceValue) { Name = slice.Name });
                }

                var legacyVariables = new VariablesList(slices);

                foreach (var input in Config.Settings.InputSettings.CustomInputs)
                {
                    legacyVariables.Set(new StringVariable(input.DefaultAnswer) { Name = input.VariableName });
                }

                data.SetObject("legacyVariables", legacyVariables);
            }

            sw.Start();
            Status = ConfigDebuggerStatus.Running;
            StatusChanged?.Invoke(this, ConfigDebuggerStatus.Running);

            if (Config.Mode != ConfigMode.Legacy)
            {
                // If the startup script is not empty, execute it
                if (!string.IsNullOrWhiteSpace(Config.StartupCSharpScript))
                {
                    // This data is temporary and will not be persisted to the bots, it is
                    // only used in this context to be able to use variables e.g. data.SOURCE
                    // and other things like providers, settings, logger.
                    // By default it doesn't support proxies.
                    var startupData = new BotData(providers, Config.Settings, Logger,
                        new DataLine(string.Empty, wordlistType), null, false)
                    {
                        CancellationToken = cts.Token,
                        Stepper = stepper
                    };

                    Logger.Log("Executing startup script...");
                    var startupGlobals = new ScriptGlobals(startupData, globals);
                    await startupScript.RunAsync(startupGlobals, null, cts.Token).ConfigureAwait(false);
                    Logger.Log("Executing main script...");
                }

                var state = await script.RunAsync(scriptGlobals, null, cts.Token).ConfigureAwait(false);

                if (Options.StepByStep)
                {
                    UpdateVariablesFromStepByStepState(state);
                }
                else
                {
                    UpdateVariablesFromScriptState(state);
                }
            }
            else
            {
                // [LEGACY] Run the LoliScript in the old way
                var loliScript = new LoliScript(Config.LoliScript);
                var lsGlobals = new LSGlobals(data);

                do
                {
                    if (cts.IsCancellationRequested)
                    {
                        break;
                    }

                    await loliScript.TakeStep(lsGlobals).ConfigureAwait(false);

                    UpdateVariablesFromLegacyState(lsGlobals);

                    if (Options.StepByStep && loliScript.CanProceed)
                    {
                        await data.Stepper!.WaitForStepAsync(data.CancellationToken).ConfigureAwait(false);
                    }
                }
                while (loliScript.CanProceed);
            }
        }
        catch (OperationCanceledException)
        {
            if (data is not null)
            {
                data.STATUS = "ERROR";
            }

            Logger.Log("Operation canceled", LogColors.Tomato);
        }
        catch (Exception ex)
        {
            if (data is not null)
            {
                data.STATUS = "ERROR";
            }

            var logErrorMessage = RuriLibSettings.RuriLibSettings.GeneralSettings.VerboseMode
                ? ex.ToString()
                : ex.Message;

            var executionInfo = data?.ExecutionInfo ?? "Debugger setup";
            Logger.Log($"[{executionInfo}] {ex.GetType().Name}: {logErrorMessage}", LogColors.Tomato);
            throw;
        }
        finally
        {
            sw.Stop();

            if (data is not null)
            {
                Logger.Log($"BOT ENDED AFTER {sw.ElapsedMilliseconds} ms WITH STATUS: {data.STATUS}");

                // Save the browsers for later use
                lastPuppeteerBrowser = data.TryGetObject<Browser>("puppeteer");
                lastPlaywright = data.TryGetObject<Microsoft.Playwright.IPlaywright>("playwright");
                lastPlaywrightBrowser = data.TryGetObject<Microsoft.Playwright.IBrowser>("playwrightBrowser");
                lastSeleniumBrowser = data.TryGetObject<OpenQA.Selenium.WebDriver>("selenium");

                // Dispose stuff in data.Objects
                data.DisposeObjectsExcept(
                [
                    "puppeteer",
                    "puppeteerPage",
                    "puppeteerFrame",
                    "puppeteerGhostCursor",
                    "playwright",
                    "playwrightBrowser",
                    "playwrightContext",
                    "playwrightPage",
                    "playwrightFrame",
                    "playwrightGhostCursor",
                    "playwrightUserAgent",
                    "browserGhostCursorRandomMovesEnabled",
                    "selenium",
                    "pythonRuntime"
                ]);
                data.AsyncLocker?.Dispose();
            }
            else
            {
                Logger.Log($"BOT ENDED AFTER {sw.ElapsedMilliseconds} ms WITH STATUS: ERROR");
            }

            // Dispose resources
            foreach (var resource in resources.Where(r => r.Value is IDisposable)
                .Select(r => r.Value).Cast<IDisposable>())
            {
                resource.Dispose();
            }

            Status = ConfigDebuggerStatus.Idle;
            StatusChanged?.Invoke(this, ConfigDebuggerStatus.Idle);
        }
    }

    /// <summary>
    /// Tries to take a step. Returns true if a step was taken.
    /// </summary>
    public bool TryTakeStep()
    {
        if (stepper == null || !stepper.IsWaiting)
        {
            return false;
        }

        StatusChanged?.Invoke(this, ConfigDebuggerStatus.Running);
        return stepper.TryTakeStep();
    }

    /// <summary>
    /// Requests cancellation of the current debug session.
    /// </summary>
    public void Stop() => cts?.Cancel();

    private void LogInputValidationWarnings(DataLine dataLine)
    {
        ArgumentNullException.ThrowIfNull(dataLine);

        if (!dataLine.IsValid)
        {
            Logger.Log("WARNING: The test input data did not respect the validity regex for the selected wordlist type!",
                LogColors.DarkOrange);
        }

        if (!TryValidateDataRules(dataLine, out var dataRulesWarning))
        {
            Logger.Log(dataRulesWarning, LogColors.DarkOrange);
        }
    }

    private bool TryValidateDataRules(DataLine dataLine, out string dataRulesWarning)
    {
        ArgumentNullException.ThrowIfNull(dataLine);

        dataRulesWarning = string.Empty;

        if (Config.Settings.DataSettings.DataRules.Count == 0)
        {
            return true;
        }

        var variables = dataLine.GetVariables();

        foreach (var rule in Config.Settings.DataSettings.DataRules)
        {
            var slice = variables.FirstOrDefault(v => v.Name == rule.SliceName);

            if (slice is null)
            {
                dataRulesWarning = $"WARNING: Could not validate the test input against the data rules because the slice '{rule.SliceName}' does not exist!";
                return false;
            }

            if (!rule.IsSatisfied(slice.AsString()))
            {
                dataRulesWarning = "WARNING: The test input data did not respect the data rules of this config!";
                return false;
            }
        }

        return true;
    }

    // Propagate the events
    private void OnNewEntry(object? sender, BotLoggerEntry entry) => NewLogEntry?.Invoke(this, entry);
    private void OnWaitingForStep(object? sender, EventArgs e)
    {
        if (Config.Mode != ConfigMode.Legacy)
        {
            UpdateVariablesFromSnapshot();
        }

        Status = ConfigDebuggerStatus.WaitingForStep;
        StatusChanged?.Invoke(this, ConfigDebuggerStatus.WaitingForStep);
    }

    private void UpdateVariablesFromLegacyState(LSGlobals lsGlobals)
    {
        ArgumentNullException.ThrowIfNull(lsGlobals);

        var updatedVariables = new List<Variable>();
        var legacyVariables = data?.TryGetObject<VariablesList>("legacyVariables");

        if (legacyVariables is not null)
        {
            updatedVariables.AddRange(legacyVariables.Variables);
        }

        updatedVariables.AddRange(lsGlobals.Globals.Variables);
        SetVariables(updatedVariables);
    }

    private void UpdateVariablesFromScriptState(ScriptState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var updatedVariables = new List<Variable>();

        foreach (var scriptVar in state.Variables)
        {
            try
            {
                var type = DescriptorsRepository.ToVariableType(scriptVar.Type);

                if (type.HasValue && !scriptVar.Name.StartsWith("tmp_"))
                {
                    var variable = DescriptorsRepository.ToVariable(scriptVar.Name, scriptVar.Type, scriptVar.Value);
                    variable.MarkedForCapture = data?.MarkedForCapture.Contains(scriptVar.Name) == true;
                    updatedVariables.Add(variable);
                }
            }
            catch
            {
                // The type is not supported, e.g. it was generated using custom C# code and not blocks
                // so we just disregard it
            }
        }

        SetVariables(updatedVariables);
    }

    private void UpdateVariablesFromStepByStepState(ScriptState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var updatedVariables = new List<Variable>();
        var snapshotVariableNames = new HashSet<string>(StringComparer.Ordinal);

        if (data is not null)
        {
            foreach (var snapshot in DebuggerVariableSnapshot.Get(data))
            {
                var variable = TryCreateVariable(snapshot);

                if (variable is null)
                {
                    continue;
                }

                updatedVariables.Add(variable);
                snapshotVariableNames.Add(variable.Name);
            }
        }

        foreach (var scriptVar in state.Variables)
        {
            try
            {
                var type = DescriptorsRepository.ToVariableType(scriptVar.Type);

                if (type.HasValue
                    && !scriptVar.Name.StartsWith("tmp_")
                    && !snapshotVariableNames.Contains(scriptVar.Name))
                {
                    var variable = DescriptorsRepository.ToVariable(scriptVar.Name, scriptVar.Type, scriptVar.Value);
                    variable.MarkedForCapture = data?.MarkedForCapture.Contains(scriptVar.Name) == true;
                    updatedVariables.Add(variable);
                }
            }
            catch
            {
                // The type is not supported, e.g. it was generated using custom C# code and not blocks
                // so we just disregard it
            }
        }

        SetVariables(updatedVariables);
    }

    private void UpdateVariablesFromSnapshot()
    {
        if (data is null)
        {
            return;
        }

        var updatedVariables = new List<Variable>();

        foreach (var snapshot in DebuggerVariableSnapshot.Get(data))
        {
            var variable = TryCreateVariable(snapshot);

            if (variable is not null)
            {
                updatedVariables.Add(variable);
            }
        }

        SetVariables(updatedVariables);
    }

    private Variable? TryCreateVariable(DebuggerVariableSnapshotEntry snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Name.StartsWith("tmp_", StringComparison.Ordinal))
        {
            return null;
        }

        var type = Nullable.GetUnderlyingType(snapshot.Type) ?? snapshot.Type;

        Variable? variable = null;

        if (type == typeof(string) && snapshot.Value is string stringValue)
        {
            variable = new StringVariable(stringValue);
        }
        else if (type == typeof(int) && snapshot.Value is int intValue)
        {
            variable = new IntVariable(intValue);
        }
        else if (type == typeof(long) && snapshot.Value is long longValue)
        {
            variable = new IntVariable(longValue);
        }
        else if (type == typeof(float) && snapshot.Value is float floatValue)
        {
            variable = new FloatVariable(floatValue);
        }
        else if (type == typeof(double) && snapshot.Value is double doubleValue)
        {
            variable = new FloatVariable(doubleValue);
        }
        else if (type == typeof(bool) && snapshot.Value is bool boolValue)
        {
            variable = new BoolVariable(boolValue);
        }
        else if (type == typeof(List<string>))
        {
            variable = new ListOfStringsVariable(snapshot.Value as List<string>);
        }
        else if (type == typeof(Dictionary<string, string>))
        {
            variable = new DictionaryOfStringsVariable(snapshot.Value as Dictionary<string, string>);
        }
        else if (type == typeof(byte[]))
        {
            variable = new ByteArrayVariable(snapshot.Value as byte[]);
        }

        if (variable is null)
        {
            return null;
        }

        variable.Name = snapshot.Name;
        variable.MarkedForCapture = data?.MarkedForCapture.Contains(snapshot.Name) == true;
        return variable;
    }

    private void SetVariables(IEnumerable<Variable> variables)
    {
        ArgumentNullException.ThrowIfNull(variables);

        Options.Variables.Clear();
        Options.Variables.AddRange(variables);
        VariablesChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Logger.NewEntry -= OnNewEntry;

        if (stepper is not null)
        {
            stepper.WaitingForStep -= OnWaitingForStep;
        }

        lastPuppeteerBrowser?.Dispose();
        lastPlaywright?.Dispose();
        lastSeleniumBrowser?.Dispose();

        GC.SuppressFinalize(this);
    }
}
