using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.CodeAnalysis.Scripting;
using RuriLib.Helpers;
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
using RuriLib.Threading;
using RuriLib.Threading.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs
{
    public class MultiRunJob : Job
    {
        // Options
        public int Bots { get; set; } = 1;
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
        public BotData[] CurrentBotDatas { get; set; } = new BotData[200];
        public bool DebuggingMode { get; set; } = true;

        // Getters
        public override float Progress => parallelizer != null ? parallelizer.Progress : -1;
        public TimeSpan Elapsed => parallelizer != null ? parallelizer.Elapsed : TimeSpan.Zero;
        public TimeSpan Remaining => parallelizer != null ? parallelizer.Remaining : Timeout.InfiniteTimeSpan;
        public int CPM => parallelizer != null ? parallelizer.CPM : 0;

        // Private fields
        private Parallelizer<MultiRunInput, CheckResult> parallelizer;
        private ProxyPool proxyPool;
        private Timer tickTimer;
        private dynamic globalVariables;
        private Timer proxyReloadTimer;

        // Instance properties and stats
        public List<Hit> Hits { get; private set; } = new List<Hit>();

        // Events
        public event EventHandler<ErrorDetails<MultiRunInput>> OnTaskError;
        public event EventHandler<ResultDetails<MultiRunInput, CheckResult>> OnResult;
        public event EventHandler<Exception> OnError;
        public event EventHandler<float> OnProgress;
        public event EventHandler<JobStatus> OnStatusChanged;
        public event EventHandler OnCompleted;
        public event EventHandler OnTimerTick;

        /*********
         * STATS *
         *********/
        
        // -- Data
        public int DataTested { get; private set; } = 0;
        public int DataHits { get; private set; } = 0;
        public int DataCustom { get; private set; } = 0;
        public int DataFails { get; private set; } = 0;
        public int DataRetried { get; private set; } = 0;
        public int DataBanned { get; private set; } = 0;
        public int DataToCheck { get; private set; } = 0;
        public int DataInvalid { get; private set; } = 0;
        public int DataErrors { get; private set; } = 0;

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
            new Func<MultiRunInput, CancellationToken, Task<CheckResult>>(async (input, token) =>
            {
                var botData = input.BotData;

                // Check if the data respects rules
                if (!botData.Line.IsValid || !botData.Line.RespectsRules(botData.ConfigSettings.DataSettings.DataRules))
                {
                    botData.STATUS = "INVALID";

                    // RETURN THE RESULT
                    return new CheckResult
                    {
                        BotData = botData,
                        ScriptVariables = new ImmutableArray<ScriptVariable>()
                    };
                }

                botData.CancellationToken = token;
                ScriptState scriptState = null;

                // Add this BotData to the array for detailed MultiRunJob display mode
                input.Job.CurrentBotDatas[(int)(input.Index++ % input.Job.Bots)] = botData;

                START:
                token.ThrowIfCancellationRequested();

                try
                {
                    // This is important! Otherwise we reuse the same proxy
                    botData.Proxy = null;

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
                                await input.ProxyPool.ReloadAll();
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

                    botData.Logger.Log($"[{DateTime.Now.ToShortTimeString()}] BOT STARTED WITH DATA {botData.Line.Data} AND PROXY {botData.Proxy}");
                    scriptState = await input.Script.RunAsync(scriptGlobals, null, token).ConfigureAwait(false);
                    botData.Logger.Log($"[{DateTime.Now.ToShortTimeString()}] BOT ENDED WITH STATUS: {botData.STATUS}");
                }
                catch
                {
                    botData.STATUS = "ERROR";
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
                    input.Job.DebugLog($"RETRY ({botData.Line.Data})({botData.Proxy})");
                    input.Job.DataRetried++;
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
                        input.Job.DataBanned++;
                        goto START;
                    }
                }

                // RETURN THE RESULT
                return new CheckResult 
                {
                    BotData = botData, 
                    ScriptVariables = scriptState != null ? scriptState.Variables : new ImmutableArray<ScriptVariable>()
                };
            });
        #endregion

        #region Controls
        public override async Task Start()
        {
            if (Config == null)
                throw new NullReferenceException("The Config cannot be null");

            if (DataPool == null)
                throw new NullReferenceException("The DataPool cannot be null");

            if (ShouldUseProxies(ProxyMode, Config.Settings.ProxySettings) && (ProxySources == null || ProxySources.Count == 0))
                throw new NullReferenceException("The list of proxy sources cannot be null or empty when proxies are needed");

            if (!Config.Settings.DataSettings.AllowedWordlistTypes.Contains(DataPool.WordlistType))
                throw new NotSupportedException("This config does not support the provided Wordlist Type");

            if (ShouldUseProxies(ProxyMode, Config.Settings.ProxySettings))
            {
                var proxyPoolOptions = new ProxyPoolOptions { AllowedTypes = Config.Settings.ProxySettings.AllowedProxyTypes };
                proxyPool = new ProxyPool(ProxySources, proxyPoolOptions);
                await proxyPool.ReloadAll(ShuffleProxies);

                if (!proxyPool.Proxies.Any())
                {
                    throw new Exception("No proxies that respect the allowed types are available, but the job is set to use proxies");
                }
            }

            // Wait for the start condition to be verified
            await base.Start();

            // If we're in LoliCode mode, build the Stack
            if (Config.Mode == ConfigMode.LoliCode)
                Config.Stack = Loli2StackTranspiler.Transpile(Config.LoliCodeScript);

            // Build the C# script
            Config.CSharpScript = Stack2CSharpTranspiler.Transpile(Config.Stack, Config.Settings);
            var script = new ScriptBuilder().Build(Config.CSharpScript, Config.Settings.ScriptSettings, pluginRepo);
            script.Compile();

            // Wait for the start condition to be verified
            // await base.Start();

            var wordlistType = settings.Environment.WordlistTypes.FirstOrDefault(t => t.Name == DataPool.WordlistType);
            globalVariables = new ExpandoObject();

            if (wordlistType == null)
                throw new NullReferenceException($"The wordlist type with name {DataPool.WordlistType} was not found in the Environment");

            var client = new HttpClient();

            var runtime = Python.CreateRuntime();
            var pyengine = runtime.GetEngine("py");
            var pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;

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
                    Script = script,
                    CustomInputsAnswers = CustomInputsAnswers,
                    Index = index++
                };

                input.BotData.Objects.Add("httpClient", client); // Add the default HTTP client
                input.BotData.Objects.Add("ironPyEngine", pyengine); // Add the IronPython engine

                return input;
            }
            );
            parallelizer = ParallelizerFactory<MultiRunInput, CheckResult>
                .Create(settings.RuriLibSettings.GeneralSettings.ParallelizerType, workItems, workFunction, Bots, DataPool.Size, Skip);
            parallelizer.NewResult += DataProcessed;
            parallelizer.StatusChanged += StatusChanged;
            parallelizer.TaskError += PropagateTaskError;
            parallelizer.Error += PropagateError;
            parallelizer.NewResult += PropagateResult;
            parallelizer.Completed += PropagateCompleted;

            ResetStats();
            StartTimers();
            logger?.LogInfo(Id, "All set, starting the execution");
            await parallelizer.Start();
        }

        public override async Task Stop()
        {
            try
            {
                await parallelizer?.Stop();
            }
            finally
            {
                StopTimers();
                logger?.LogInfo(Id, "Execution stopped");
            }
        }

        public override async Task Abort()
        {
            try
            {
                await parallelizer?.Abort();
            }
            finally
            {
                StopTimers();
                logger?.LogInfo(Id, "Execution aborted");
            }
        }

        public override async Task Pause()
        {
            try
            {
                await parallelizer?.Pause();
            }
            finally
            {
                StopTimers();
                logger?.LogInfo(Id, "Execution paused");
            }
        }

        public override async Task Resume()
        {
            await parallelizer?.Resume();
            StartTimers();
            logger?.LogInfo(Id, "Execution resumed");
        }
        #endregion

        #region Public Methods
        public async Task FetchProxiesFromSources()
            => await proxyPool.ReloadAll(ShuffleProxies);
        #endregion

        #region Wrappers for TaskManager methods
        public async Task ChangeBots(int amount)
        {
            if (parallelizer != null)
            {
                await parallelizer.ChangeDegreeOfParallelism(amount);
                logger?.LogInfo(Id, $"Changed bots to {amount}");
            }
        }
        #endregion

        #region Propagation of TaskManager events
        private void PropagateTaskError(object sender, ErrorDetails<MultiRunInput> details)
        {
            OnTaskError?.Invoke(sender, details);
            logger?.LogException(Id, details.Exception);
        }

        private void PropagateError(object sender, Exception ex)
        {
            OnError?.Invoke(sender, ex);
            logger?.LogException(Id, ex);
        }

        private void PropagateResult(object sender, ResultDetails<MultiRunInput, CheckResult> result)
        {
            OnResult?.Invoke(sender, result);
            // We're not logging results to the IJobLogger because they could arrive at a very high rate
            // and not be very useful, we're mostly interested in errors here.
        }

        private void PropagateProgress(object sender, float progress)
        {
            OnProgress?.Invoke(sender, progress);
        }

        private void PropagateCompleted(object sender, EventArgs e)
        {
            OnCompleted?.Invoke(sender, e);
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
                proxyReloadTimer = new Timer(new TimerCallback(async _ => await proxyPool.ReloadAll(ShuffleProxies)),
                    null, (int)PeriodicReloadInterval.TotalMilliseconds, (int)PeriodicReloadInterval.TotalMilliseconds);
            }
        }

        private void StopTimers()
        {
            tickTimer?.Dispose();
            proxyReloadTimer?.Dispose();
        }

        private void ResetStats()
        {
            DataTested = 0;
            DataHits = 0;
            DataCustom = 0;
            DataFails = 0;
            DataRetried = 0;
            DataBanned = 0;
            DataToCheck = 0;
            DataErrors = 0;
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
                case "SUCCESS": DataHits++; break;
                case "NONE": DataToCheck++; break;
                case "FAIL": DataFails++; break;
                case "INVALID": DataInvalid++; break;
                default: DataCustom++; break;
            }

            DataTested++;
        }

        private async Task RegisterHit(CheckResult result)
        {
            var botData = result.BotData;

            var hit = new Hit()
            {
                Data = botData.Line,
                BotLogger = settings.RuriLibSettings.GeneralSettings.EnableBotLogging ? botData.Logger : null,
                Type = botData.STATUS,
                DataPool = DataPool,
                Config = Config,
                Date = DateTime.Now,
                Proxy = botData.Proxy,
                CapturedData = new Dictionary<string, object>(),
                OwnerId = OwnerId
            };

            foreach (var variable in result.ScriptVariables)
            {
                if (botData.MarkedForCapture.Contains(variable.Name))
                {
                    hit.CapturedData.Add(variable.Name, variable.Value);
                }
            }

            // Add it to the local list of hits
            Hits.Add(hit);

            foreach (var hitOutput in HitOutputs)
            {
                await hitOutput.Store(hit).ConfigureAwait(false);
            }
        }

        private bool ShouldUseProxies(JobProxyMode mode, ProxySettings settings)
        {
            return mode switch
            {
                JobProxyMode.Default => settings.UseProxies,
                JobProxyMode.On => true,
                JobProxyMode.Off => false,
                _ => throw new NotImplementedException()
            };
        }

        private bool IsHitStatus(string status)
            => status != "FAIL" && status != "RETRY" && status != "BAN" && status != "ERROR";

        private void DebugLog(string message)
        {
            if (DebuggingMode)
            {
                Console.WriteLine($"[{DateTime.Now}] {message}");
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
        public Dictionary<string, string> CustomInputsAnswers { get; set; }
        public long Index { get; set; }
    }

    public struct CheckResult
    {
        public BotData BotData { get; set; }
        public ImmutableArray<ScriptVariable> ScriptVariables { get; set; }
    }
}
