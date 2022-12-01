using IronPython.Hosting;
using Microsoft.CodeAnalysis.Scripting;
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
using IronPython.Compiler;
using IronPython.Runtime;
using RuriLib.Models.Captchas;
using RuriLib.Legacy.Models;
using RuriLib.Legacy.LS;
using RuriLib.Models.Variables;

namespace RuriLib.Models.Jobs
{
    public class MultiRunJob : Job
    {
        // Options
        public int Bots { get; set; } = 1;
        public int BotLimit { get; init; } = 200;
        public int Skip { get; set; } = 0;
        public Config Config { get; set; }
        public DataPool DataPool { get; set; }
        public List<ProxySource> ProxySources { get; set; } = new List<ProxySource>();
        public JobProxyMode ProxyMode { get; set; } = JobProxyMode.Default;
        public bool ShuffleProxies { get; set; } = true;
        public NoValidProxyBehaviour NoValidProxyBehaviour { get; set; } = NoValidProxyBehaviour.Reload;
        public TimeSpan ProxyBanTime { get; set; } = TimeSpan.Zero;
        public bool MarkAsToCheckOnAbort { get; set; } = false;
        public bool NeverBanProxies { get; set; } = false;
        public bool ConcurrentProxyMode { get; set; } = false;
        public TimeSpan PeriodicReloadInterval { get; set; } = TimeSpan.Zero;
        public List<IHitOutput> HitOutputs { get; set; } = new List<IHitOutput>();
        public Bots.Providers Providers { get; set; }
        public TimeSpan TickInterval = TimeSpan.FromMinutes(1);
        public Dictionary<string, string> CustomInputsAnswers { get; set; } = new Dictionary<string, string>();
        public BotData[] CurrentBotDatas { get; set; }

        // Getters
        public override float Progress => parallelizer?.Progress ?? -1;
        public TimeSpan Elapsed => parallelizer?.Elapsed ?? TimeSpan.Zero;
        public TimeSpan Remaining => parallelizer?.Remaining ?? Timeout.InfiniteTimeSpan;
        public int CPM => parallelizer?.CPM ?? 0;

        // Private fields
        private readonly string[] badStatuses = new string[] { "FAIL", "RETRY", "BAN", "ERROR", "INVALID" };
        private Parallelizer<MultiRunInput, CheckResult> parallelizer;
        private ProxyPool proxyPool;
        private Timer tickTimer;
        private dynamic globalVariables;
        private VariablesList legacyGlobalVariables;
        private Dictionary<string, string> legacyGlobalCookies;
        private Dictionary<string, ConfigResource> resources;
        private HttpClient httpClient;
        private AsyncLocker asyncLocker;
        private Timer proxyReloadTimer;

        // Instance properties and stats
        public List<Hit> Hits { get; private set; } = new List<Hit>();

        // Events
        public event EventHandler<ErrorDetails<MultiRunInput>> OnTaskError;
        public event EventHandler<ResultDetails<MultiRunInput, CheckResult>> OnResult;
        public event EventHandler<Exception> OnError;
        public event EventHandler<float> OnProgress;
        public event EventHandler<JobStatus> OnStatusChanged;
        public event EventHandler OnBotsChanged;
        public event EventHandler OnCompleted;
        public event EventHandler OnTimerTick;
        public event EventHandler<Hit> OnHit;

        /*********
         * STATS *
         *********/

        // -- Data
        private int dataTested = 0;
        public int DataTested => dataTested;

        private int dataHits = 0;
        public int DataHits => dataHits;

        private int dataCustom = 0;
        public int DataCustom => dataCustom;

        private int dataFails = 0;
        public int DataFails => dataFails;

        private int dataRetried = 0;
        public int DataRetried => dataRetried;

        private int dataBanned = 0;
        public int DataBanned => dataBanned;

        private int dataToCheck = 0;
        public int DataToCheck => dataToCheck;

        private int dataInvalid = 0;
        public int DataInvalid => dataInvalid;

        private int dataErrors = 0;
        public int DataErrors => dataErrors;

        // -- Proxies
        public int ProxiesTotal => proxyPool == null ? 0 : proxyPool.Proxies.Count();
        public int ProxiesAlive => proxyPool == null ? 0 : proxyPool.Proxies
            .Count(p => p.ProxyStatus == ProxyStatus.Available || p.ProxyStatus == ProxyStatus.Busy);
        public int ProxiesBanned => proxyPool == null ? 0 : proxyPool.Proxies.Count(p => p.ProxyStatus == ProxyStatus.Banned);
        public int ProxiesBad => proxyPool == null ? 0 : proxyPool.Proxies.Count(p => p.ProxyStatus == ProxyStatus.Bad);

        // -- Misc
        public decimal CaptchaCredit { get; private set; } = 0;

        public MultiRunJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
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
                ScriptState scriptState = null;
                LSGlobals lsGlobals = null; // Legacy

                if (input.IsLegacy)
                {
                    lsGlobals = new LSGlobals(botData)
                    {
                        Globals = input.LegacyGlobals,
                        GlobalCookies = input.LegacyGlobalCookies
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
                input.Job.CurrentBotDatas[botIndex] = botData;

                // Set the BOTNUM
                botData.BOTNUM = botIndex + 1;

                START:
                token.ThrowIfCancellationRequested();
                botData.ResetState();

                try
                {
                    // This is important! Otherwise we reuse the same proxy
                    botData.Proxy = null;
                    botData.UseProxy = ShouldUseProxies(input.Job.ProxyMode, botData.ConfigSettings.ProxySettings);

                    // Get a hold of a proxy
                    if (botData.UseProxy)
                    {
                        GETPROXY:
                        token.ThrowIfCancellationRequested();

                        lock (input.ProxyPool)
                        {
                            botData.Proxy = input.ProxyPool.GetProxy(input.Job.ConcurrentProxyMode,
                                input.BotData.ConfigSettings.ProxySettings.MaxUsesPerProxy);
                        }

                        if (botData.Proxy == null)
                        {
                            if (input.Job.NoValidProxyBehaviour == NoValidProxyBehaviour.Reload)
                            {
                                try
                                {
                                    await input.BotData.AsyncLocker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync),
                                        input.BotData.CancellationToken).ConfigureAwait(false);
                                    
                                    botData.Proxy = input.ProxyPool.GetProxy(input.Job.ConcurrentProxyMode, input.BotData.ConfigSettings.ProxySettings.MaxUsesPerProxy);
                                    
                                    if (botData.Proxy == null)
                                    {
                                        await input.ProxyPool.ReloadAllAsync(true, token).ConfigureAwait(false);
                                    }
                                }
                                finally
                                {
                                    input.BotData.AsyncLocker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
                                }
                            }
                            else if (input.Job.NoValidProxyBehaviour == NoValidProxyBehaviour.Unban)
                            {
                                input.ProxyPool.UnbanAll(input.Job.ProxyBanTime);
                            }
                            
                            goto GETPROXY;
                        }
                    }

                    var scriptGlobals = new ScriptGlobals(botData, input.Globals);

                    // Set custom inputs answers
                    foreach (var answer in input.CustomInputsAnswers)
                        (scriptGlobals.input as IDictionary<string, object>).Add(answer.Key, answer.Value);

                    botData.Logger.Log($"[{DateTime.Now.ToLongTimeString()}] BOT STARTED WITH DATA {botData.Line.Data} AND PROXY {botData.Proxy}");

                    // If it's a DLL config, invoke the method
                    if (input.IsDLL)
                    {
                        var task = (Task)input.DLLMethod.Invoke(null, new object[]
                        {
                            botData,
                            scriptGlobals.input,
                            scriptGlobals.globals,
                            outputVariables,
                            token
                        });

                        await task.ConfigureAwait(false);
                        
                    }
                    // If it's a legacy config, run the LoliScript engine
                    else if (input.IsLegacy)
                    {
                        var loliScript = new LoliScript(input.LegacyLoliScript);

                        do
                        {
                            if (botData.CancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            await loliScript.TakeStep(lsGlobals);
                        }
                        while (loliScript.CanProceed);
                    }
                    // Otherwise run the compiled script
                    else
                    {
                        scriptState = await input.Script.RunAsync(scriptGlobals, null, token).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
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
                        botData.DisposeObjectsExcept(new[] { "httpClient", "ironPyEngine" });
                    }
                    else
                    {
                        botData.DisposeObjectsExcept(new[] { "puppeteer", "puppeteerPage", "puppeteerFrame", "httpClient", "ironPyEngine" });
                    }
                }

                // Update captcha credit
                if (botData.CaptchaCredit > 0)
                {
                    input.Job.CaptchaCredit = botData.CaptchaCredit;
                }

                if (botData.Proxy != null)
                {
                    // If a ban status occurred, ban the proxy
                    if (input.BotData.ConfigSettings.ProxySettings.BanProxyStatuses.Contains(botData.STATUS))
                        input.ProxyPool.ReleaseProxy(input.BotData.Proxy, !input.Job.NeverBanProxies);

                    // Otherwise set it to available
                    else if (botData.Proxy.ProxyStatus == ProxyStatus.Busy)
                        input.ProxyPool.ReleaseProxy(input.BotData.Proxy, false);
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
                                await botData.Providers.Captcha.ReportSolution(long.Parse(lastCaptcha.Id), lastCaptcha.Type, false, cts.Token).ConfigureAwait(false);
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

                    foreach (var variable in legacyVariables.Variables.Where(v => v.MarkedForCapture))
                    {
                        switch (variable.Type)
                        {
                            case VariableType.String:
                                outputVariables[variable.Name] = variable.AsString();
                                break;

                            case VariableType.ListOfStrings:
                                outputVariables[variable.Name] = variable.AsListOfStrings();
                                break;

                            case VariableType.DictionaryOfStrings:
                                outputVariables[variable.Name] = variable.AsDictionaryOfStrings();
                                break;
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
        public override async Task Start(CancellationToken cancellationToken = default)
        {
            if (Status is JobStatus.Starting or JobStatus.Running)
                throw new Exception("Job already started");

            try
            {
                Status = JobStatus.Starting;
                asyncLocker = new();

                if (Config == null)
                    throw new NullReferenceException("The Config cannot be null");

                if (DataPool == null)
                    throw new NullReferenceException("The DataPool cannot be null");

                if (Skip >= DataPool.Size)
                    throw new ArgumentException("The skip must be smaller than the total number of lines in the data pool");

                if (ShouldUseProxies(ProxyMode, Config.Settings.ProxySettings) && (ProxySources == null || ProxySources.Count == 0))
                    throw new NullReferenceException("The list of proxy sources cannot be null or empty when proxies are needed");

                if (!Config.Settings.DataSettings.AllowedWordlistTypes.Contains(DataPool.WordlistType))
                    throw new NotSupportedException("This config does not support the provided Wordlist Type");

                if (ShouldUseProxies(ProxyMode, Config.Settings.ProxySettings))
                {
                    // HACK: This should probably not be here, but it will work for now
                    ProxySources.ForEach(p => p.UserId = OwnerId);

                    var proxyPoolOptions = new ProxyPoolOptions { AllowedTypes = Config.Settings.ProxySettings.AllowedProxyTypes };
                    proxyPool = new ProxyPool(ProxySources, proxyPoolOptions);
                    try
                    {
                        await asyncLocker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync), cancellationToken).ConfigureAwait(false);
                        await proxyPool.ReloadAllAsync(ShuffleProxies, cancellationToken).ConfigureAwait(false);
                    }
                    finally
                    {
                        asyncLocker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
                    }

                    if (!proxyPool.Proxies.Any())
                    {
                        throw new Exception("No proxies that respect the allowed types are available, but the job is set to use proxies");
                    }
                }

                // Wait for the start condition to be verified
                await base.Start(cancellationToken).ConfigureAwait(false);

                // Execute the startup script
                if (Config.Mode == ConfigMode.LoliCode || Config.Mode == ConfigMode.Stack)
                {
                    Config.StartupCSharpScript = Loli2CSharpTranspiler.Transpile(Config.StartupLoliCodeScript, Config.Settings);
                }

                Script script = null;
                MethodInfo method = null;

                // If not in DLL mode, build the C# script and compile it
                if (Config.Mode == ConfigMode.DLL)
                {
                    using var ms = new MemoryStream(Config.DLLBytes);
                    var assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
                    var type = assembly.GetType("RuriLib.CompiledConfig");
                    method = type.GetMember("Execute").First() as MethodInfo;
                }
                else if (Config.Mode == ConfigMode.Legacy)
                {
                    // Nothing to do here
                }
                else
                {
                    switch (Config.Mode)
                    {
                        case ConfigMode.Stack:
                            Config.CSharpScript = Stack2CSharpTranspiler.Transpile(Config.Stack, Config.Settings);
                            break;

                        case ConfigMode.LoliCode:
                            Config.CSharpScript = Loli2CSharpTranspiler.Transpile(Config.LoliCodeScript, Config.Settings);
                            break;
                    }

                    script = new ScriptBuilder().Build(Config.CSharpScript, Config.Settings.ScriptSettings, pluginRepo);
                    script.Compile(cancellationToken);
                }

                Providers.Security.X509RevocationMode = Config.Mode == ConfigMode.DLL
                    ? System.Security.Cryptography.X509Certificates.X509RevocationMode.Online
                    : System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;

                var wordlistType = settings.Environment.WordlistTypes.FirstOrDefault(t => t.Name == DataPool.WordlistType);
                globalVariables = new ExpandoObject();
                legacyGlobalVariables = new();
                legacyGlobalCookies = new();

                if (wordlistType == null)
                    throw new NullReferenceException($"The wordlist type with name {DataPool.WordlistType} was not found in the Environment");

                resources = new();

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
                        throw new Exception($"Could not create resource {opt.Name}");
                    }
                }

                globalVariables.Resources = resources;
                httpClient = new();
                var runtime = Python.CreateRuntime();
                var pyengine = runtime.GetEngine("py");
                var pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
                pco.Module &= ~ModuleOptions.Optimized;

                if (!string.IsNullOrWhiteSpace(Config.StartupCSharpScript))
                {
                    var startupScript = new ScriptBuilder().Build(Config.StartupCSharpScript, Config.Settings.ScriptSettings, pluginRepo);
                    var startupBotData = new BotData(Providers, Config.Settings, new BotLogger(), new DataLine(string.Empty, wordlistType), null, false);
                    var startupGlobals = new ScriptGlobals(startupBotData, globalVariables);
                    await startupScript.RunAsync(startupGlobals, null, cancellationToken).ConfigureAwait(false);
                }

                long index = 0;
                var workItems = DataPool.DataList.Select(line =>
                {
                    var input = new MultiRunInput
                    {
                        Job = this,
                        ProxyPool = proxyPool,
                        BotData = new BotData(Providers, Config.Settings, new BotLogger(), new DataLine(line, wordlistType),
                            null, ShouldUseProxies(ProxyMode, Config.Settings.ProxySettings)),
                        Globals = globalVariables,
                        LegacyLoliScript = Config.LoliScript,
                        LegacyGlobals = legacyGlobalVariables,
                        LegacyGlobalCookies = legacyGlobalCookies,
                        Script = script,
                        IsDLL = Config.Mode == ConfigMode.DLL,
                        IsLegacy = Config.Mode == ConfigMode.Legacy,
                        DLLMethod = method,
                        CustomInputsAnswers = CustomInputsAnswers,
                        Index = index++
                    };

                    input.BotData.Logger.Enabled = settings.RuriLibSettings.GeneralSettings.EnableBotLogging && Config.Mode != ConfigMode.DLL;
                    input.BotData.SetObject("httpClient", httpClient); // Add the default HTTP client
                    input.BotData.SetObject("ironPyEngine", pyengine); // Add the IronPython engine
                    input.BotData.AsyncLocker = asyncLocker;

                    return input;
                });

                parallelizer = ParallelizerFactory<MultiRunInput, CheckResult>
                    .Create(settings.RuriLibSettings.GeneralSettings.ParallelizerType, workItems,
                        workFunction, Bots, DataPool.Size, Skip, BotLimit);

                parallelizer.CPMLimit = Config.Settings.GeneralSettings.MaximumCPM;
                parallelizer.NewResult += DataProcessed;
                parallelizer.StatusChanged += StatusChanged;
                parallelizer.TaskError += PropagateTaskError;
                parallelizer.Error += PropagateError;
                parallelizer.NewResult += PropagateResult;
                parallelizer.Completed += PropagateCompleted;
                parallelizer.Completed += (s, e) =>
                {
                    Skip += DataTested;
                };

                ResetStats();
                StartTimers();
                logger?.LogInfo(Id, "All set, starting the execution");
                await parallelizer.Start().ConfigureAwait(false);
            }
            finally
            {
                // Reset the status
                if (Status == JobStatus.Starting)
                {
                    Status = JobStatus.Idle;
                }
            }
        }

        public override async Task Stop()
        {
            try
            {
                if (parallelizer is not null)
                {
                    await parallelizer.Stop().ConfigureAwait(false);
                }
            }
            finally
            {
                StopTimers();
                logger?.LogInfo(Id, "Execution stopped");
                DisposeGlobals();
            }
        }

        public override async Task Abort()
        {
            try
            {
                if (parallelizer is not null)
                {
                    await parallelizer.Abort().ConfigureAwait(false);
                }
            }
            finally
            {
                StopTimers();
                logger?.LogInfo(Id, "Execution aborted");
                DisposeGlobals();
            }
        }

        public override async Task Pause()
        {
            try
            {
                if (parallelizer is not null)
                {
                    await parallelizer.Pause().ConfigureAwait(false);
                }
            }
            finally
            {
                StopTimers();
                logger?.LogInfo(Id, "Execution paused");
            }
        }

        public override async Task Resume()
        {
            if (parallelizer is not null)
            {
                await parallelizer.Resume().ConfigureAwait(false);
            }

            StartTimers();
            logger?.LogInfo(Id, "Execution resumed");
        }
        #endregion

        #region Public Methods
        public async Task FetchProxiesFromSources(CancellationToken cancellationToken = default)
        {
            try
            {
                await asyncLocker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync), cancellationToken).ConfigureAwait(false);
                await proxyPool.ReloadAllAsync(ShuffleProxies).ConfigureAwait(false);
            }
            finally
            {
                asyncLocker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
            }

        }

        #endregion

        #region Wrappers for Parallelizer methods
        public async Task ChangeBots(int amount)
        {
            if (parallelizer != null)
            {
                await parallelizer.ChangeDegreeOfParallelism(amount).ConfigureAwait(false);
                logger?.LogInfo(Id, $"Changed bots to {amount}");
                OnBotsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Propagation of Parallelizer events
        private void PropagateTaskError(object _, ErrorDetails<MultiRunInput> details)
        {
            OnTaskError?.Invoke(this, details);
            logger?.LogException(Id, details.Exception);
        }

        private void PropagateError(object _, Exception ex)
        {
            OnError?.Invoke(this, ex);
            logger?.LogException(Id, ex);
        }

        private void PropagateResult(object _, ResultDetails<MultiRunInput, CheckResult> result)
        {
            OnResult?.Invoke(this, result);

            if (!settings.RuriLibSettings.GeneralSettings.LogAllResults) return;
            
            var data = result.Result.BotData;
            logger?.LogInfo(Id, $"[{data.STATUS}] {data.Line.Data} ({data.Proxy})");
        }

        private void PropagateProgress(object _, float progress)
        {
            OnProgress?.Invoke(this, progress);
        }

        private void PropagateCompleted(object _, EventArgs e)
        {
            OnCompleted?.Invoke(this, e);
            logger?.LogInfo(Id, "Execution completed");
        }
        #endregion

        #region Private Methods
        private void StartTimers()
        {
            tickTimer = new Timer(new TimerCallback(_ => OnTimerTick?.Invoke(this, EventArgs.Empty)),
                null, (int)TickInterval.TotalMilliseconds, (int)TickInterval.TotalMilliseconds);

            if (PeriodicReloadInterval > TimeSpan.Zero)
            {
                proxyReloadTimer = new Timer(new TimerCallback(async _ =>
                {
                    // BEWARE: Fire-and-forget async-void delegate
                    // Unhandled exceptions will crash the process
                    if (proxyPool is not null)
                    {
                        try
                        {
                            await asyncLocker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync), CancellationToken.None)
                                .ConfigureAwait(false);
                            await proxyPool.ReloadAllAsync(ShuffleProxies).ConfigureAwait(false);
                        }
                        catch
                        {
                            // ignored
                        }
                        finally
                        {
                            asyncLocker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
                        }
                    }
                }), null, (int)PeriodicReloadInterval.TotalMilliseconds, (int)PeriodicReloadInterval.TotalMilliseconds);
            }
        }

        private void StopTimers()
        {
            tickTimer?.Dispose();
            proxyReloadTimer?.Dispose();
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
            dataErrors = 0;
            Hits = new();
        }

        private void StatusChanged(object sender, ParallelizerStatus status)
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

            OnStatusChanged?.Invoke(this, Status);
        }

        private void DataProcessed(object sender, ResultDetails<MultiRunInput, CheckResult> details)
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

            if (parallelizer.Status == ParallelizerStatus.Stopping)
            {
                details.Item.BotData.ExecutionInfo = "STOPPED";
            }
        }

        private async Task RegisterHit(CheckResult result)
        {
            var botData = result.BotData;

            var hit = new Hit()
            {
                Data = botData.Line,
                BotLogger = settings.RuriLibSettings.GeneralSettings.EnableBotLogging && Config.Mode != ConfigMode.DLL 
                    ? botData.Logger 
                    : null,
                Type = botData.STATUS,
                DataPool = DataPool,
                Config = Config,
                Date = DateTime.Now,
                Proxy = botData.Proxy,
                CapturedData = Config.Settings.GeneralSettings.SaveEmptyCaptures
                    ? result.OutputVariables : CleanEmptyCaptures(result.OutputVariables),
                OwnerId = OwnerId
            };

            // Add it to the local list of hits
            Hits.Add(hit);
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

        private bool IsHitStatus(string status) => !badStatuses.Contains(status);

        private void DebugLog(string message)
        {
            if (Providers.GeneralSettings.VerboseMode)
            {
                Console.WriteLine($"[{DateTime.Now}] {message}");
            }
        }

        private void DisposeGlobals()
        {
            if (httpClient is not null)
            {
                try
                {
                    httpClient.Dispose();
                }
                catch
                {

                }
            }

            if (asyncLocker is not null)
            {
                try
                {
                    asyncLocker.Dispose();
                }
                catch
                {

                }
            }

            proxyPool?.Dispose();

            if (ProxySources is not null)
            {
                for (int i = 0; i < ProxySources.Count; i++)
                {
                    ProxySources[i]?.Dispose();
                }
            }

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

                    }
                }
            }
        }
        #endregion
    }

    public struct MultiRunInput
    {
        public MultiRunJob Job { get; set; }
        public BotData BotData { get; set; }
        public dynamic Globals { get; set; }
        public ProxyPool ProxyPool { get; set; }
        public Script Script { get; set; }
        public bool IsDLL { get; set; }
        public bool IsLegacy { get; set; }
        public string LegacyLoliScript { get; set; }
        public VariablesList LegacyGlobals { get; set; }
        public Dictionary<string, string> LegacyGlobalCookies { get; set; }
        public MethodInfo DLLMethod { get; set; }
        public Dictionary<string, string> CustomInputsAnswers { get; set; }
        public long Index { get; set; }
    }

    public struct CheckResult
    {
        public BotData BotData { get; set; }
        public Dictionary<string, object> OutputVariables { get; set; }
    }
}
