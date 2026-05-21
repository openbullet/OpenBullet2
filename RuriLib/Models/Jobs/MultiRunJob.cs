using IronPython.Hosting;
using Microsoft.CodeAnalysis.Scripting;
using PuppeteerSharp;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Transpilers;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data;
using RuriLib.Models.Hits;
using RuriLib.Models.Proxies;
using RuriLib.Services;
using RuriLib.Parallelization;
using RuriLib.Parallelization.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using RuriLib.Models.Data.Resources;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Helpers;
using RuriLib.Blocks.Interop;
using IronPython.Compiler;
using IronPython.Runtime;
using RuriLib.Models.Captchas;
using RuriLib.Legacy.Models;
using RuriLib.Legacy.LS;
using RuriLib.Models.Variables;
using RuriLib.Proxies.Exceptions;

namespace RuriLib.Models.Jobs;

/// <summary>
/// Represents a multi-run job that executes a config against many data lines.
/// </summary>
public class MultiRunJob : Job
{
    // Options
    /// <summary>Gets or sets the number of worker bots.</summary>
    public int Bots { get; set; } = 1;
    /// <summary>Gets the maximum allowed number of worker bots.</summary>
    public int BotLimit { get; init; } = 200;
    /// <summary>Gets or sets the number of lines to skip before processing.</summary>
    public int Skip { get; set; }
    /// <summary>Gets or sets the config to execute.</summary>
    public Config? Config { get; set; }
    /// <summary>Gets or sets the data pool to consume.</summary>
    public DataPool? DataPool { get; set; }
    /// <summary>Gets or sets the proxy sources used by the job.</summary>
    public List<ProxySource> ProxySources { get; set; } = [];
    /// <summary>Gets or sets how proxies should be used.</summary>
    public JobProxyMode ProxyMode { get; set; } = JobProxyMode.Default;
    /// <summary>Gets or sets a value indicating whether proxies should be shuffled after reload.</summary>
    public bool ShuffleProxies { get; set; } = true;
    /// <summary>Gets or sets the behavior to apply when no valid proxy is available.</summary>
    public NoValidProxyBehaviour NoValidProxyBehaviour { get; set; } = NoValidProxyBehaviour.Reload;
    /// <summary>Gets or sets the ban duration used when unbanning proxies.</summary>
    public TimeSpan ProxyBanTime { get; set; } = TimeSpan.Zero;
    /// <summary>Gets or sets a value indicating whether aborted lines should be marked as to-check.</summary>
    public bool MarkAsToCheckOnAbort { get; set; }
    /// <summary>Gets or sets a value indicating whether proxies should never be banned.</summary>
    public bool NeverBanProxies { get; set; }
    /// <summary>Gets or sets a value indicating whether bad proxy failures should keep the legacy ban behavior.</summary>
    public bool NeverMarkProxiesAsBad { get; set; }
    /// <summary>Gets or sets a value indicating whether busy proxies may be reused concurrently.</summary>
    public bool ConcurrentProxyMode { get; set; }
    /// <summary>Gets or sets the periodic proxy reload interval.</summary>
    public TimeSpan PeriodicReloadInterval { get; set; } = TimeSpan.Zero;
    /// <summary>Gets or sets the hit outputs used to persist results.</summary>
    public List<IHitOutput> HitOutputs { get; set; } = [];
    /// <summary>Gets or sets the runtime providers used by worker bots.</summary>
    public Bots.Providers? Providers { get; set; }
    /// <summary>Gets or sets the timer tick interval.</summary>
    public TimeSpan TickInterval = TimeSpan.FromSeconds(1);
    /// <summary>Gets or sets the answers for configured custom inputs.</summary>
    public Dictionary<string, string> CustomInputsAnswers { get; set; } = [];
    /// <summary>Gets or sets the current bot snapshots.</summary>
    public BotData[] CurrentBotDatas { get; set; } = [];

    // Getters
    /// <inheritdoc />
    public override float Progress => parallelizer?.Progress ?? -1;
    /// <inheritdoc />
    public override TimeSpan Elapsed => parallelizer?.Elapsed ?? TimeSpan.Zero;
    /// <inheritdoc />
    public override TimeSpan Remaining => parallelizer?.Remaining ?? Timeout.InfiniteTimeSpan;
    /// <summary>Gets the current checks per minute.</summary>
    public int CPM => parallelizer?.CPM ?? 0;

    // Private fields
    private readonly string[] badStatuses = ["FAIL", "RETRY", "BAN", "ERROR", "INVALID"];
    private readonly object hitsLock = new();
    private bool disposed;
    private Parallelizer<MultiRunInput, CheckResult>? parallelizer;
    private ProxyPool? proxyPool;
    private Timer? tickTimer;
    private dynamic? globalVariables;
    private VariablesList? legacyGlobalVariables;
    private Dictionary<string, string>? legacyGlobalCookies;
    private Dictionary<string, ConfigResource>? resources;
    private HttpClient? httpClient;
    private AsyncLocker? asyncLocker;
    private Timer? proxyReloadTimer;
    private CancellationTokenSource? startCts;
    private JobLastRunOutcome pendingLastRunOutcome = JobLastRunOutcome.None;

    // Instance properties and stats
    /// <summary>Gets the hits collected during the current run.</summary>
    public List<Hit> Hits { get; private set; } = [];

    /// <summary>
    /// Gets a snapshot of the hits collected during the current run.
    /// </summary>
    public List<Hit> GetHitsSnapshot()
    {
        lock (hitsLock)
        {
            return [.. Hits];
        }
    }

    /// <summary>
    /// Finds a hit by id in a thread-safe manner.
    /// </summary>
    public Hit? FindHit(string id)
    {
        lock (hitsLock)
        {
            return Hits.Find(h => h.Id == id);
        }
    }

    // Events
    /// <summary>Raised when a worker task fails.</summary>
    public event EventHandler<ErrorDetails<MultiRunInput>>? OnTaskError;
    /// <summary>Raised when a worker result is produced.</summary>
    public event EventHandler<ResultDetails<MultiRunInput, CheckResult>>? OnResult;
    /// <summary>Raised when the job encounters an error.</summary>
    public event EventHandler<Exception>? OnError;
    /// <summary>Raised when the job emits a log entry.</summary>
    public event EventHandler<BotLoggerEntry>? OnLogEntry;
    /// <summary>Raised when progress changes.</summary>
    public event EventHandler<float>? OnProgress;
    /// <summary>Raised when the job status changes.</summary>
    public event EventHandler<JobStatus>? OnStatusChanged;
    /// <summary>Raised when the bot count changes.</summary>
    public event EventHandler? OnBotsChanged;
    /// <summary>Raised when the job completes.</summary>
    public event EventHandler? OnCompleted;
    /// <summary>Raised on each timer tick.</summary>
    public event EventHandler? OnTimerTick;
    /// <summary>Raised when a hit is registered.</summary>
    public event EventHandler<Hit>? OnHit;

    /*********
     * STATS *
     *********/

    // -- Data
    private int dataTested;
    /// <summary>Gets the number of processed data lines.</summary>
    public int DataTested => dataTested;

    private int dataHits;
    /// <summary>Gets the number of successful lines.</summary>
    public int DataHits => dataHits;

    private int dataCustom;
    /// <summary>Gets the number of custom-status lines.</summary>
    public int DataCustom => dataCustom;

    private int dataFails;
    /// <summary>Gets the number of failed lines.</summary>
    public int DataFails => dataFails;

    private int dataRetried;
    /// <summary>Gets the number of retried lines.</summary>
    public int DataRetried => dataRetried;

    private int dataBanned;
    /// <summary>Gets the number of banned lines.</summary>
    public int DataBanned => dataBanned;

    private int dataToCheck;
    /// <summary>Gets the number of lines marked as to-check.</summary>
    public int DataToCheck => dataToCheck;

    private int dataInvalid;
    /// <summary>Gets the number of invalid lines.</summary>
    public int DataInvalid => dataInvalid;

    private int dataErrors;
    /// <summary>Gets the number of errored lines.</summary>
    public int DataErrors => dataErrors;

    // -- Proxies
    /// <summary>Gets the total number of proxies in the pool.</summary>
    public int ProxiesTotal => proxyPool == null ? 0 : proxyPool.Proxies.Count();
    /// <summary>Gets the number of available or busy proxies.</summary>
    public int ProxiesAlive => proxyPool == null ? 0 : proxyPool.Proxies
        .Count(p => p.ProxyStatus == ProxyStatus.Available || p.ProxyStatus == ProxyStatus.Busy);
    /// <summary>Gets the number of banned proxies.</summary>
    public int ProxiesBanned => proxyPool == null ? 0 : proxyPool.Proxies.Count(p => p.ProxyStatus == ProxyStatus.Banned);
    /// <summary>Gets the number of bad proxies.</summary>
    public int ProxiesBad => proxyPool == null ? 0 : proxyPool.Proxies.Count(p => p.ProxyStatus == ProxyStatus.Bad);

    // -- Misc
    /// <summary>Gets the latest available captcha credit.</summary>
    public decimal CaptchaCredit { get; private set; }

    /// <summary>
    /// Creates a multi-run job.
    /// </summary>
    /// <param name="settings">The RuriLib settings service.</param>
    /// <param name="pluginRepo">The plugin repository.</param>
    /// <param name="logger">The optional job logger.</param>
    public MultiRunJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger? logger = null)
        : base(settings, pluginRepo, logger)
    {

    }

    #region Work Function
    private Func<MultiRunInput, CancellationToken, Task<CheckResult>> workFunction =
        new(async (input, token) =>
        {
            var botData = input.BotData;
            botData.CancellationToken = token;

            // Check if the data respects rules
            if (!botData.Line.IsValid || !botData.Line.RespectsRules(botData.ConfigSettings.DataSettings.DataRules))
            {
                botData.STATUS = "INVALID";

                // RETURN THE RESULT
                return new CheckResult
                {
                    BotData = botData,
                    OutputVariables = new()
                };
            }

            botData.CancellationToken = token;
            ScriptState? scriptState = null;
            LSGlobals? lsGlobals = null; // Legacy

            if (input.IsLegacy)
            {
                lsGlobals = new LSGlobals(botData)
                {
                    Globals = input.LegacyGlobals ?? new(),
                    GlobalCookies = input.LegacyGlobalCookies ?? []
                };

                var slices = new List<Variable>();

                foreach (var slice in botData.Line.GetVariables())
                {
                    var sliceValue = botData.ConfigSettings.DataSettings.UrlEncodeDataAfterSlicing
                        ? Uri.EscapeDataString(slice.AsString())
                        : slice.AsString();

                    slices.Add(new StringVariable(sliceValue) { Name = slice.Name });
                }

                var legacyVariables = new VariablesList(slices);

                foreach (var customInput in botData.ConfigSettings.InputSettings.CustomInputs)
                {
                    legacyVariables.Set(new StringVariable(customInput.DefaultAnswer) { Name = customInput.VariableName });
                }

                botData.SetObject("legacyVariables", legacyVariables);
            }

            Dictionary<string, object> outputVariables = new();

            // Add this BotData to the array for detailed MultiRunJob display mode
            var botIndex = (int)(input.Index++ % input.Job.Bots);
            if (botIndex < input.Job.CurrentBotDatas.Length)
            {
                input.Job.CurrentBotDatas[botIndex] = botData;
            }

            // Set the BOTNUM
            botData.BOTNUM = botIndex + 1;

            START:
            token.ThrowIfCancellationRequested();
            botData.ResetState();
            var badProxyFailure = false;

            try
            {
                // This is important! Otherwise we reuse the same proxy
                botData.Proxy = null;
                botData.UseProxy = ShouldUseProxies(input.Job.ProxyMode, botData.ConfigSettings.ProxySettings);

                // Get a hold of a proxy
                if (botData.UseProxy)
                {
                    var inputProxyPool = input.ProxyPool
                        ?? throw new InvalidOperationException("The proxy pool was not initialized");

                    GETPROXY:
                    token.ThrowIfCancellationRequested();

                    lock (inputProxyPool)
                    {
                        botData.Proxy = inputProxyPool.GetProxy(input.Job.ConcurrentProxyMode,
                            input.BotData.ConfigSettings.ProxySettings.MaxUsesPerProxy);
                    }

                    if (botData.Proxy == null)
                    {
                        if (input.Job.NoValidProxyBehaviour == NoValidProxyBehaviour.Reload)
                        {
                            try
                            {
                                var locker = input.BotData.AsyncLocker
                                    ?? throw new InvalidOperationException("The async locker was not initialized");
                                await locker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync),
                                    input.BotData.CancellationToken).ConfigureAwait(false);

                                botData.Proxy = inputProxyPool.GetProxy(input.Job.ConcurrentProxyMode, input.BotData.ConfigSettings.ProxySettings.MaxUsesPerProxy);

                                if (botData.Proxy == null)
                                {
                                    await inputProxyPool.ReloadAllAsync(true, token).ConfigureAwait(false);
                                }
                            }
                            finally
                            {
                                var locker = input.BotData.AsyncLocker
                                    ?? throw new InvalidOperationException("The async locker was not initialized");
                                locker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
                            }
                        }
                        else if (input.Job.NoValidProxyBehaviour == NoValidProxyBehaviour.Unban)
                        {
                            inputProxyPool.UnbanAll(input.Job.ProxyBanTime);
                        }

                        goto GETPROXY;
                    }
                }

                var scriptGlobals = new ScriptGlobals(botData, input.Globals);
                var inputValues = scriptGlobals.input as IDictionary<string, object?>
                    ?? throw new InvalidOperationException("The script input object does not support key/value access");

                // Set custom inputs answers
                foreach (var answer in input.CustomInputsAnswers ?? [])
                {
                    inputValues.Add(answer.Key, answer.Value);
                }

                botData.Logger.Log($"[{DateTime.Now.ToLongTimeString()}] BOT STARTED WITH DATA {botData.Line.Data} AND PROXY {botData.Proxy}");

                // If it's a DLL config, invoke the method
                if (input.IsDLL)
                {
                    var dllMethod = input.DLLMethod
                        ?? throw new InvalidOperationException("The DLL method cannot be null");
                    var task = dllMethod.Invoke(null, new object[]
                    {
                            botData,
                            scriptGlobals.input,
                            scriptGlobals.globals,
                            outputVariables,
                            token
                    }) as Task
                        ?? throw new InvalidOperationException("The DLL Execute method must return a Task");

                    await task.ConfigureAwait(false);

                }
                // If it's a legacy config, run the LoliScript engine
                else if (input.IsLegacy)
                {
                    var legacyLoliScript = input.LegacyLoliScript
                        ?? throw new InvalidOperationException("The legacy script cannot be null");
                    var loliScript = new LoliScript(legacyLoliScript);

                    do
                    {
                        if (botData.CancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        await loliScript.TakeStep(lsGlobals!);
                    }
                    while (loliScript.CanProceed);
                }
                // Otherwise run the compiled script
                else
                {
                    var script = input.Script
                        ?? throw new InvalidOperationException("The compiled script cannot be null");
                    scriptState = await script.RunAsync(scriptGlobals, null, token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                badProxyFailure = IsBadProxyFailure(ex);
                botData.STATUS = "ERROR";
                botData.Logger.Log($"[{botData.ExecutionInfo}] {ex.GetType().Name}: {ex.Message}", LogColors.Tomato);
                Interlocked.Increment(ref input.Job.dataErrors);
            }
            finally
            {
                var endMessage = $"[{DateTime.Now.ToLongTimeString()}] BOT ENDED WITH STATUS: {botData.STATUS}";
                botData.ExecutingBlock(endMessage);
                botData.Logger.Log(endMessage);

                // Close the browser if needed
                if (botData.ConfigSettings.BrowserSettings.QuitBrowserStatuses.Contains(botData.STATUS))
                {
                    await ClosePuppeteerBrowserIfOpen(botData).ConfigureAwait(false);
                    await ClosePlaywrightBrowserIfOpen(botData).ConfigureAwait(false);
                    botData.DisposeObjectsExcept(new[] { "httpClient", "ironPyEngine", "pythonRuntime" });
                }
                else
                {
                    botData.DisposeObjectsExcept(
                    [
                        "puppeteer",
                        "puppeteerPage",
                        "puppeteerFrame",
                        "playwright",
                        "playwrightBrowser",
                        "playwrightContext",
                        "playwrightPage",
                        "playwrightFrame",
                        "playwrightUserAgent",
                        "httpClient",
                        "ironPyEngine",
                        "pythonRuntime"
                    ]);
                }
            }

            // Update captcha credit
            if (botData.CaptchaCredit > 0)
            {
                input.Job.CaptchaCredit = botData.CaptchaCredit;
            }

            if (botData.Proxy != null)
            {
                if (badProxyFailure && !input.Job.NeverMarkProxiesAsBad)
                {
                    input.ProxyPool?.ReleaseProxy(botData.Proxy, ProxyStatus.Bad);
                }
                // If a ban status occurred, ban the proxy
                else if (input.BotData.ConfigSettings.ProxySettings.BanProxyStatuses.Contains(botData.STATUS))
                    input.ProxyPool?.ReleaseProxy(botData.Proxy, !input.Job.NeverBanProxies);

                // Otherwise set it to available
                else if (botData.Proxy.ProxyStatus == ProxyStatus.Busy)
                    input.ProxyPool?.ReleaseProxy(botData.Proxy, false);
            }

            // If we aborted
            if (token.IsCancellationRequested)
            {
                // Optionally send to tocheck and return the result normally
                if (input.Job.MarkAsToCheckOnAbort)
                {
                    input.Job.DebugLog($"TO CHECK ON ABORT ({botData.Line.Data})({botData.Proxy})");
                    botData.STATUS = "NONE";
                }
                // Otherwise just throw
                else
                {
                    input.Job.DebugLog("TASK HARD CANCELED");
                    throw new TaskCanceledException();
                }
            }
            else if (botData.STATUS == "RETRY")
            {
                if (botData.ConfigSettings.GeneralSettings.ReportLastCaptchaOnRetry)
                {
                    var lastCaptcha = botData.TryGetObject<CaptchaInfo>("lastCaptchaInfo");

                    if (lastCaptcha is not null)
                    {
                        try
                        {
                            botData.ExecutingBlock("Reporting bad captcha upon RETRY status");
                            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            await botData.Providers.Captcha.ReportSolutionAsync(
                                lastCaptcha.Id, lastCaptcha.Type, false, cts.Token).ConfigureAwait(false);
                            botData.ExecutingBlock("Bad captcha reported!");
                        }
                        catch
                        {

                        }
                    }
                }

                input.Job.DebugLog($"RETRY ({botData.Line.Data})({botData.Proxy})");
                Interlocked.Increment(ref input.Job.dataRetried);
                goto START;
            }
            else if (botData.STATUS == "BAN" || botData.STATUS == "ERROR")
            {
                botData.Line.Retries++;
                var evasion = botData.ConfigSettings.ProxySettings.BanLoopEvasion;

                if (evasion > 0 && botData.Line.Retries > evasion)
                {
                    botData.STATUS = "NONE";
                    input.Job.DebugLog($"TO CHECK ON BAN LOOP EVASION ({botData.Line.Data})({botData.Proxy})");
                }
                else
                {
                    input.Job.DebugLog($"BAN ({botData.Line.Data})({botData.Proxy})");
                    Interlocked.Increment(ref input.Job.dataBanned);
                    goto START;
                }
            }

            if (input.IsDLL)
            {
                // No need to do anything here, DLL configs already fill the output variables
            }
            else if (input.IsLegacy)
            {
                var legacyVariables = botData.TryGetObject<VariablesList>("legacyVariables");
                if (legacyVariables is not null)
                {
                    foreach (var variable in legacyVariables.Variables.Where(v => v.MarkedForCapture))
                    {
                        switch (variable.Type)
                        {
                            case VariableType.String:
                                outputVariables[variable.Name] = variable.AsString();
                                break;

                            case VariableType.ListOfStrings:
                                outputVariables[variable.Name] = variable.AsListOfStrings() ?? [];
                                break;

                            case VariableType.DictionaryOfStrings:
                                outputVariables[variable.Name] = variable.AsDictionaryOfStrings() ?? [];
                                break;
                        }
                    }
                }
            }
            else if (scriptState != null && !scriptState.Variables.IsDefault)
            {
                // Get the variables from the script
                foreach (var variable in scriptState.Variables)
                {
                    if (botData.MarkedForCapture.Contains(variable.Name))
                    {
                        outputVariables[variable.Name] = variable.Value;
                    }
                }
            }

            // RETURN THE RESULT
            return new CheckResult
            {
                BotData = botData,
                OutputVariables = outputVariables
            };
        });
    #endregion

    #region Controls
    /// <inheritdoc />
    public override async Task Start(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (Status is JobStatus.Starting or JobStatus.Running)
            throw new Exception("Job already started");

        try
        {
            LastRunOutcome = JobLastRunOutcome.None;
            pendingLastRunOutcome = JobLastRunOutcome.None;
            ResetForNewRun();
            startCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, startCts.Token);

            Status = JobStatus.Starting;
            OnStatusChanged?.Invoke(this, Status);

            asyncLocker = new();

            var config = Config ?? throw new InvalidOperationException("The Config cannot be null");
            var dataPool = DataPool ?? throw new InvalidOperationException("The DataPool cannot be null");
            var providers = Providers ?? throw new InvalidOperationException("The Providers cannot be null");

            if (Skip >= dataPool.Size)
                throw new ArgumentException(
                    "The skip must be smaller than the total number of lines in the data pool");

            // Reload the data pool from the source
            dataPool.Reload();

            if (ShouldUseProxies(ProxyMode, config.Settings.ProxySettings) && ProxySources.Count == 0)
                throw new InvalidOperationException(
                    "The list of proxy sources cannot be null or empty when proxies are needed");

            if (!config.Settings.DataSettings.AllowedWordlistTypes.Contains(dataPool.WordlistType))
                throw new NotSupportedException("This config does not support the provided Wordlist Type");

            if (ShouldUseProxies(ProxyMode, config.Settings.ProxySettings))
            {
                // HACK: This should probably not be here, but it will work for now
                ProxySources.ForEach(p => p.UserId = OwnerId);

                var proxyPoolOptions =
                    new ProxyPoolOptions { AllowedTypes = config.Settings.ProxySettings.AllowedProxyTypes };
                proxyPool = new ProxyPool(ProxySources, proxyPoolOptions,
                    logger is null ? null : new JobLoggerAdapter<ProxyPool>(logger, Id));
                try
                {
                    await asyncLocker
                        .Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync), linkedCts.Token)
                        .ConfigureAwait(false);
                    await proxyPool.ReloadAllAsync(ShuffleProxies, linkedCts.Token).ConfigureAwait(false);
                }
                finally
                {
                    asyncLocker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
                }

                if (!proxyPool.Proxies.Any())
                {
                    throw new Exception(
                        "No proxies that respect the allowed types are available, but the job is set to use proxies");
                }
            }

            Status = JobStatus.Waiting;
            OnStatusChanged?.Invoke(this, Status);

            // Wait for the start condition to be verified
            await base.Start(linkedCts.Token).ConfigureAwait(false);

            Status = JobStatus.Starting;
            OnStatusChanged?.Invoke(this, Status);

            // Execute the startup script
            if (config.Mode == ConfigMode.LoliCode || config.Mode == ConfigMode.Stack)
            {
                config.StartupCSharpScript =
                    Loli2CSharpTranspiler.Transpile(config.StartupLoliCodeScript, config.Settings);
            }

            Script? script = null;
            MethodInfo? method = null;

            // If not in DLL mode, build the C# script and compile it
            if (config.Mode == ConfigMode.DLL)
            {
                using var ms = new MemoryStream(config.DLLBytes);
                var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                var type = assembly.GetType("RuriLib.CompiledConfig")
                    ?? throw new InvalidOperationException("The DLL config does not contain RuriLib.CompiledConfig");
                method = type.GetMethod("Execute")
                    ?? throw new InvalidOperationException("The DLL config does not expose an Execute method");
            }
            else if (config.Mode == ConfigMode.Legacy)
            {
                // Nothing to do here
            }
            else
            {
                switch (config.Mode)
                {
                    case ConfigMode.Stack:
                        config.CSharpScript = Stack2CSharpTranspiler.Transpile(config.Stack, config.Settings);
                        break;

                    case ConfigMode.LoliCode:
                        config.CSharpScript =
                            Loli2CSharpTranspiler.Transpile(config.LoliCodeScript, config.Settings);
                        break;
                }

                script = new ScriptBuilder().Build(config.CSharpScript, config.Settings.ScriptSettings, pluginRepo);
                script.Compile(linkedCts.Token);
            }

            providers.Security.X509RevocationMode = config.Mode == ConfigMode.DLL
                ? System.Security.Cryptography.X509Certificates.X509RevocationMode.Online
                : System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

            var wordlistType =
                settings.Environment.WordlistTypes.FirstOrDefault(t => t.Name == dataPool.WordlistType);
            globalVariables = new ExpandoObject();
            legacyGlobalVariables = new();
            legacyGlobalCookies = new();

            if (wordlistType == null)
                throw new NullReferenceException(
                    $"The wordlist type with name {dataPool.WordlistType} was not found in the Environment");

            resources = new();

            // Resources will need to be disposed of
            foreach (var opt in config.Settings.DataSettings.Resources)
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
                    throw new Exception($"Could not create resource {opt.Name}");
                }
            }

            globalVariables.Resources = resources;
            globalVariables.OwnerId = OwnerId;
            globalVariables.JobId = Id;
            httpClient = new();
            var runtime = Python.CreateRuntime();
            var pyengine = runtime.GetEngine("py");
            var pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;
            var pythonRuntime = PythonScriptRuntime.GetShared();

            if (!string.IsNullOrWhiteSpace(config.StartupCSharpScript))
            {
                var startupScript = new ScriptBuilder().Build(config.StartupCSharpScript,
                    config.Settings.ScriptSettings, pluginRepo);
                var startupLogger = new BotLogger();
                startupLogger.NewEntry += PropagateLogEntry;
                var startupBotData =
                    new BotData(providers, config.Settings, startupLogger,
                        new DataLine(string.Empty, wordlistType), null, false)
                    {
                        CancellationToken = linkedCts.Token
                    };

                try
                {
                    startupLogger.Log("Executing startup script...");
                    var startupGlobals = new ScriptGlobals(startupBotData, globalVariables);
                    await startupScript.RunAsync(startupGlobals, null, linkedCts.Token).ConfigureAwait(false);
                    startupLogger.Log("Executing main script...");
                }
                finally
                {
                    startupLogger.NewEntry -= PropagateLogEntry;
                }
            }

            linkedCts.Token.ThrowIfCancellationRequested();

            CurrentBotDatas = new BotData[Bots];
            long index = 0;
            var workItems = dataPool.DataList.Select(line =>
            {
                var input = new MultiRunInput
                {
                    Job = this,
                    ProxyPool = proxyPool,
                    BotData = new BotData(providers, config.Settings, new BotLogger(),
                        new DataLine(line, wordlistType),
                        null, ShouldUseProxies(ProxyMode, config.Settings.ProxySettings)),
                    Globals = globalVariables,
                    LegacyLoliScript = config.LoliScript,
                    LegacyGlobals = legacyGlobalVariables,
                    LegacyGlobalCookies = legacyGlobalCookies,
                    Script = script,
                    IsDLL = config.Mode == ConfigMode.DLL,
                    IsLegacy = config.Mode == ConfigMode.Legacy,
                    DLLMethod = method,
                    CustomInputsAnswers = CustomInputsAnswers,
                    Index = index++
                };

                input.BotData.Logger.Enabled = settings.RuriLibSettings.GeneralSettings.EnableBotLogging &&
                                               config.Mode != ConfigMode.DLL;
                input.BotData.SetObject("httpClient", httpClient); // Add the default HTTP client
                input.BotData.SetObject("ironPyEngine", pyengine); // Add the IronPython engine
                input.BotData.SetObject("pythonRuntime", pythonRuntime, false); // Add the CPython runtime
                input.BotData.AsyncLocker = asyncLocker;

                return input;
            });

            linkedCts.Token.ThrowIfCancellationRequested();

            parallelizer = ParallelizerFactory<MultiRunInput, CheckResult>
                .Create(settings.RuriLibSettings.GeneralSettings.ParallelizerType, workItems,
                    workFunction, Bots, dataPool.Size, Skip, BotLimit);

            parallelizer.CPMLimit = config.Settings.GeneralSettings.MaximumCPM;
            parallelizer.NewResult += DataProcessed;
            parallelizer.StatusChanged += StatusChanged;
            parallelizer.TaskError += PropagateTaskError;
            parallelizer.Error += PropagateError;
            parallelizer.NewResult += PropagateResult;
            parallelizer.Completed += PropagateCompleted;
            parallelizer.Completed += (s, e) =>
            {
                Skip = MultiRunJobCheckpoint.GetNextSkip(Skip, DataTested, dataPool.Size);
            };

            ResetStats();
            StartTimers();
            logger?.LogInfo(Id, "All set, starting the execution");
            await parallelizer.Start().ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
        catch (Exception ex)
        {
            if (LastRunOutcome == JobLastRunOutcome.None)
            {
                LastRunOutcome = JobLastRunOutcome.Failed;
            }

            OnError?.Invoke(this, ex);
            throw;
        }
        finally
        {
            // Reset the status
            if (Status is JobStatus.Starting)
            {
                Status = JobStatus.Idle;
                OnStatusChanged?.Invoke(this, Status);
            }

            startCts?.Dispose();
            startCts = null;
        }
    }

    /// <inheritdoc />
    public override async Task Stop()
    {
        pendingLastRunOutcome = JobLastRunOutcome.Stopped;

        try
        {
            if (parallelizer is not null)
            {
                await parallelizer.Stop().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
            throw;
        }
        finally
        {
            if (LastRunOutcome == JobLastRunOutcome.None)
            {
                LastRunOutcome = JobLastRunOutcome.Stopped;
            }

            StopTimers();
            logger?.LogInfo(Id, "Execution stopped");
            DisposeRunResources();
        }
    }

    /// <inheritdoc />
    public override async Task Abort()
    {
        pendingLastRunOutcome = JobLastRunOutcome.Aborted;

        try
        {
            if (parallelizer is not null)
            {
                await parallelizer.Abort().ConfigureAwait(false);
            }

            if (startCts is not null)
            {
                await startCts.CancelAsync();
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
            throw;
        }
        finally
        {
            if (LastRunOutcome == JobLastRunOutcome.None)
            {
                LastRunOutcome = JobLastRunOutcome.Aborted;
            }

            StopTimers();
            logger?.LogInfo(Id, "Execution aborted");
            DisposeRunResources();
        }
    }

    /// <inheritdoc />
    public override async Task Pause()
    {
        try
        {
            if (parallelizer is not null)
            {
                await parallelizer.Pause().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
            throw;
        }
        finally
        {
            StopTimers();
            logger?.LogInfo(Id, "Execution paused");
        }
    }

    /// <inheritdoc />
    public override async Task Resume()
    {
        try
        {
            if (parallelizer is not null)
            {
                await parallelizer.Resume().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            OnError?.Invoke(this, ex);
            throw;
        }

        StartTimers();
        logger?.LogInfo(Id, "Execution resumed");
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Reloads proxies from the configured sources.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the reload finishes.</returns>
    public async Task FetchProxiesFromSources(CancellationToken cancellationToken = default)
    {
        var locker = asyncLocker ?? throw new InvalidOperationException("The job has not been initialized yet");
        var pool = proxyPool ?? throw new InvalidOperationException("The proxy pool has not been initialized yet");

        try
        {
            await locker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync), cancellationToken).ConfigureAwait(false);
            await pool.ReloadAllAsync(ShuffleProxies).ConfigureAwait(false);
        }
        finally
        {
            locker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
        }

    }

    #endregion

    #region Wrappers for Parallelizer methods
    /// <summary>
    /// Changes the number of worker bots used by the job.
    /// </summary>
    /// <param name="amount">The new worker count.</param>
    /// <returns>A task that completes when the change has been applied.</returns>
    public async Task ChangeBots(int amount)
    {
        if (parallelizer is not null)
        {
            await parallelizer.ChangeDegreeOfParallelism(amount).ConfigureAwait(false);
        }

        Bots = amount;
        logger?.LogInfo(Id, $"Changed bots to {amount}");
        OnBotsChanged?.Invoke(this, EventArgs.Empty);
    }
    #endregion

    #region Propagation of Parallelizer events
    private void PropagateTaskError(object? _, ErrorDetails<MultiRunInput> details)
    {
        OnTaskError?.Invoke(this, details);
        logger?.LogException(Id, details.Exception);
    }

    private void PropagateLogEntry(object? _, BotLoggerEntry entry)
        => OnLogEntry?.Invoke(this, entry);

    private void PropagateError(object? _, Exception ex)
    {
        OnError?.Invoke(this, ex);
        logger?.LogException(Id, ex);
    }

    private void PropagateResult(object? _, ResultDetails<MultiRunInput, CheckResult> result)
    {
        OnResult?.Invoke(this, result);

        if (!settings.RuriLibSettings.GeneralSettings.LogAllResults) return;

        var data = result.Result.BotData;
        logger?.LogInfo(Id, $"[{data.STATUS}] {data.Line.Data} ({data.Proxy})");
    }

    private void PropagateProgress(object? _, float progress)
    {
        OnProgress?.Invoke(this, progress);
    }

    private void PropagateCompleted(object? _, EventArgs e)
    {
        if (LastRunOutcome == JobLastRunOutcome.None && Progress >= 1f)
        {
            LastRunOutcome = JobLastRunOutcome.Completed;
        }

        StopTimers();
        OnCompleted?.Invoke(this, e);
        logger?.LogInfo(Id, "Execution completed");
    }
    #endregion

    #region Private Methods
    private void StartTimers()
    {
        tickTimer = new Timer(new TimerCallback(_ => OnTimerTick?.Invoke(this, EventArgs.Empty)),
            null, (int)TickInterval.TotalMilliseconds, (int)TickInterval.TotalMilliseconds);

        var locker = asyncLocker;

        if (PeriodicReloadInterval > TimeSpan.Zero && locker is not null)
        {
            proxyReloadTimer = new Timer(new TimerCallback(async _ =>
            {
                // BEWARE: Fire-and-forget async-void delegate
                // Unhandled exceptions will crash the process
                if (proxyPool is not null)
                {
                    try
                    {
                        await locker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync), CancellationToken.None)
                            .ConfigureAwait(false);
                        await proxyPool.ReloadAllAsync(ShuffleProxies).ConfigureAwait(false);
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        locker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
                    }
                }
            }), null, (int)PeriodicReloadInterval.TotalMilliseconds, (int)PeriodicReloadInterval.TotalMilliseconds);
        }
    }

    private void StopTimers()
    {
        tickTimer?.Dispose();
        tickTimer = null;
        proxyReloadTimer?.Dispose();
        proxyReloadTimer = null;
    }

    private void ResetStats()
    {
        dataTested = 0;
        dataHits = 0;
        dataCustom = 0;
        dataFails = 0;
        dataRetried = 0;
        dataBanned = 0;
        dataToCheck = 0;
        dataInvalid = 0;
        dataErrors = 0;

        lock (hitsLock)
        {
            Hits = [];
        }
    }

    private void StatusChanged(object? sender, ParallelizerStatus status)
    {
        Status = status switch
        {
            ParallelizerStatus.Idle => JobStatus.Idle,
            ParallelizerStatus.Starting => JobStatus.Starting,
            ParallelizerStatus.Running => JobStatus.Running,
            ParallelizerStatus.Pausing => JobStatus.Pausing,
            ParallelizerStatus.Paused => JobStatus.Paused,
            ParallelizerStatus.Stopping => JobStatus.Stopping,
            ParallelizerStatus.Resuming => JobStatus.Resuming,
            _ => throw new NotImplementedException()
        };

        if (Status == JobStatus.Idle && LastRunOutcome == JobLastRunOutcome.None)
        {
            if (Progress >= 1f)
            {
                LastRunOutcome = JobLastRunOutcome.Completed;
            }
            else if (pendingLastRunOutcome != JobLastRunOutcome.None)
            {
                LastRunOutcome = pendingLastRunOutcome;
            }
        }

        if (Status == JobStatus.Idle)
        {
            pendingLastRunOutcome = JobLastRunOutcome.None;
        }

        OnStatusChanged?.Invoke(this, Status);
    }

    private void DataProcessed(object? sender, ResultDetails<MultiRunInput, CheckResult> details)
    {
        var botData = details.Result.BotData;

        if (IsHitStatus(botData.STATUS))
        {
            // Fire and forget
            RegisterHit(details.Result).ConfigureAwait(false);
        }

        // Update the stats
        switch (botData.STATUS)
        {
            case "SUCCESS": Interlocked.Increment(ref dataHits); break;
            case "NONE": Interlocked.Increment(ref dataToCheck); break;
            case "FAIL": Interlocked.Increment(ref dataFails); break;
            case "INVALID": Interlocked.Increment(ref dataInvalid); break;
            default: Interlocked.Increment(ref dataCustom); break;
        }

        Interlocked.Increment(ref dataTested);

        if (parallelizer?.Status == ParallelizerStatus.Stopping)
        {
            details.Item.BotData.ExecutionInfo = "STOPPED";
        }
    }

    private async Task RegisterHit(CheckResult result)
    {
        var config = Config ?? throw new InvalidOperationException("The Config cannot be null");
        var dataPool = DataPool ?? throw new InvalidOperationException("The DataPool cannot be null");
        var botData = result.BotData;

        var hit = new Hit()
        {
            Data = botData.Line,
            BotLogger = settings.RuriLibSettings.GeneralSettings.EnableBotLogging && config.Mode != ConfigMode.DLL
                ? botData.Logger
                : null,
            Type = botData.STATUS,
            DataPool = dataPool,
            Config = config,
            Date = DateTime.Now,
            Proxy = botData.Proxy,
            CapturedData = config.Settings.GeneralSettings.SaveEmptyCaptures
                ? result.OutputVariables : CleanEmptyCaptures(result.OutputVariables),
            OwnerId = OwnerId
        };

        // Add it to the local list of hits
        lock (hitsLock)
        {
            Hits.Add(hit);
        }

        OnHit?.Invoke(this, hit);

        foreach (var hitOutput in HitOutputs)
        {
            await hitOutput.Store(hit).ConfigureAwait(false);
        }
    }

    private static Dictionary<string, object> CleanEmptyCaptures(Dictionary<string, object> capturedData)
    {
        var newCaptures = new Dictionary<string, object>();

        foreach (var item in capturedData)
        {
            if (item.Value is string stringValue && string.IsNullOrWhiteSpace(stringValue))
                continue;

            if (item.Value is byte[] bytesValue && bytesValue.Length == 0)
                continue;

            if (item.Value is List<string> listValue && listValue.Count == 0)
                continue;

            if (item.Value is Dictionary<string, string> dictValue && dictValue.Count == 0)
                continue;

            newCaptures.Add(item.Key, item.Value);
        }

        return newCaptures;
    }

    private static bool ShouldUseProxies(JobProxyMode mode, ProxySettings settings) => mode switch
    {
        JobProxyMode.Default => settings.UseProxies,
        JobProxyMode.On => true,
        JobProxyMode.Off => false,
        _ => throw new NotImplementedException()
    };

    /// <summary>
    /// Determines whether the current config should use proxies.
    /// </summary>
    /// <returns><see langword="true"/> if proxies should be used.</returns>
    public bool ShouldUseProxies()
    {
        var proxySettings = this.Config?.Settings.ProxySettings;
        return proxySettings != null && ShouldUseProxies(this.ProxyMode, proxySettings);
    }

    private bool IsHitStatus(string status) => !badStatuses.Contains(status);

    private static bool IsBadProxyFailure(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is BadProxyException)
            {
                return true;
            }
        }

        return false;
    }

    private void DebugLog(string message)
    {
        if (Providers?.GeneralSettings.VerboseMode == true)
        {
            logger?.LogInfo(Id, $"[{DateTime.Now}] {message}");
        }
    }

    private void ResetForNewRun()
    {
        StopTimers();
        DisposeParallelizer();
        DisposeRunResources();
    }

    private void DisposeRunResources()
    {
        TryDispose(ref httpClient);
        TryDispose(ref asyncLocker);
        TryDispose(ref proxyPool);
        DisposeResources();
        globalVariables = null;
        legacyGlobalVariables = null;
        legacyGlobalCookies = null;
    }

    private void DisposeParallelizer()
    {
        TryDispose(ref parallelizer);
    }

    private void DisposeOwnedComponents()
    {
        foreach (var hitOutput in HitOutputs.OfType<IDisposable>())
        {
            try
            {
                hitOutput.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        foreach (var proxySource in ProxySources)
        {
            try
            {
                proxySource.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }

    private void DisposeResources()
    {
        if (resources is not null)
        {
            foreach (var resource in resources.Where(r => r.Value is IDisposable)
                .Select(r => r.Value).Cast<IDisposable>())
            {
                try
                {
                    resource.Dispose();
                }
                catch
                {
                    // ignored
                }
            }

            resources = null;
        }
    }

    private static void TryDispose<T>(ref T? disposable) where T : class, IDisposable
    {
        if (disposable is null)
        {
            return;
        }

        try
        {
            disposable.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            disposable = null;
        }
    }

    private static async Task ClosePuppeteerBrowserIfOpen(BotData botData)
    {
        var browser = botData.TryGetObject<IBrowser>("puppeteer");

        if (browser is null)
        {
            return;
        }

        try
        {
            if (!browser.IsClosed)
            {
                await browser.CloseAsync().ConfigureAwait(false);
            }
        }
        catch
        {
            // ignored, the remaining tracked objects still need to be cleared
        }

        await botData.DisposeObjectAsync("puppeteer").ConfigureAwait(false);
    }

    private static async Task ClosePlaywrightBrowserIfOpen(BotData botData)
    {
        var browser = botData.TryGetObject<Microsoft.Playwright.IBrowser>("playwrightBrowser");

        if (browser is not null)
        {
            try
            {
                if (browser.IsConnected)
                {
                    await browser.CloseAsync().ConfigureAwait(false);
                }
            }
            catch
            {
                // ignored, the remaining tracked objects still need to be cleared
            }
        }

        await botData.DisposeObjectAsync("playwrightBrowser").ConfigureAwait(false);
        await botData.DisposeObjectAsync("playwright").ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposed || !disposing)
        {
            return;
        }

        StopTimers();
        DisposeParallelizer();
        DisposeRunResources();
        DisposeOwnedComponents();
        disposed = true;
        base.Dispose(disposing);
    }
    #endregion
}

/// <summary>
/// Represents the input payload processed by a multi-run worker.
/// </summary>
public struct MultiRunInput
{
    /// <summary>Gets or sets the owning job.</summary>
    public MultiRunJob Job { get; set; }
    /// <summary>Gets or sets the bot data instance.</summary>
    public BotData BotData { get; set; }
    /// <summary>Gets or sets the globals object exposed to scripts.</summary>
    public dynamic Globals { get; set; }
    /// <summary>Gets or sets the proxy pool.</summary>
    public ProxyPool? ProxyPool { get; set; }
    /// <summary>Gets or sets the compiled script.</summary>
    public Script? Script { get; set; }
    /// <summary>Gets or sets a value indicating whether the config is a DLL.</summary>
    public bool IsDLL { get; set; }
    /// <summary>Gets or sets a value indicating whether the config uses the legacy engine.</summary>
    public bool IsLegacy { get; set; }
    /// <summary>Gets or sets the legacy LoliScript payload.</summary>
    public string? LegacyLoliScript { get; set; }
    /// <summary>Gets or sets the legacy global variables.</summary>
    public VariablesList? LegacyGlobals { get; set; }
    /// <summary>Gets or sets the legacy global cookies.</summary>
    public Dictionary<string, string>? LegacyGlobalCookies { get; set; }
    /// <summary>Gets or sets the DLL entry method.</summary>
    public MethodInfo? DLLMethod { get; set; }
    /// <summary>Gets or sets the custom input answers.</summary>
    public Dictionary<string, string>? CustomInputsAnswers { get; set; }
    /// <summary>Gets or sets the worker index.</summary>
    public long Index { get; set; }
}

/// <summary>
/// Represents the result produced by a multi-run worker.
/// </summary>
public struct CheckResult
{
    /// <summary>Gets or sets the resulting bot data.</summary>
    public BotData BotData { get; set; }
    /// <summary>Gets or sets the captured output variables.</summary>
    public Dictionary<string, object> OutputVariables { get; set; }
}
