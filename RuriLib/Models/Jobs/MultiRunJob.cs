using IronPython.Compiler;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.CodeAnalysis.Scripting;
using RuriLib.Helpers.CSharp;
using RuriLib.Helpers.Transpilers;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data;
using RuriLib.Models.Hits;
using RuriLib.Models.Jobs.Threading;
using RuriLib.Models.Proxies;
using RuriLib.Models.UserAgents;
using RuriLib.Services;
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
        public List<IHitOutput> HitOutputs { get; set; } = new List<IHitOutput>();
        public IRandomUAProvider RandomUAProvider { get; set; }
        public TimeSpan TickInterval = TimeSpan.FromMinutes(1);
        public Dictionary<string, string> CustomInputsAnswers { get; set; } = new Dictionary<string, string>();
        public BotData[] CurrentBotDatas { get; set; } = new BotData[200];

        // Getters
        public override float Progress => taskManager != null ? taskManager.Progress : -1;
        public TimeSpan Elapsed => taskManager != null ? taskManager.Elapsed : TimeSpan.Zero;
        public TimeSpan Remaining => taskManager != null ? taskManager.Remaining : Timeout.InfiniteTimeSpan;
        public int CPM => taskManager != null ? taskManager.CPM : 0;

        // Private fields
        private TaskManager<MultiRunInput, CheckResult> taskManager { get; set; }
        private ProxyPool proxyPool;
        private readonly Random random;
        private Timer tickTimer;
        private dynamic globalVariables;

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
        public int DataBad { get; private set; } = 0;
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

        public MultiRunJob(RuriLibSettingsService settings, PluginRepository pluginRepo) : base(settings, pluginRepo)
        {
            // Create a random basing on the unique job id
            random = new Random(Id);
        }

        #region Work Function
        private Func<MultiRunInput, CancellationToken, Task<CheckResult>> workFunction =
            new Func<MultiRunInput, CancellationToken, Task<CheckResult>>(async (input, token) =>
            {
                var botData = input.BotData;

                // Check if the data respects rules
                if (!botData.Line.RespectsRules(botData.ConfigSettings.DataSettings.DataRules))
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
                    // Get a hold of a proxy
                    if (botData.UseProxy)
                    {
                        while (botData.Proxy == null)
                        {
                            token.ThrowIfCancellationRequested();

                            lock (input.ProxyPool)
                            {
                                botData.Proxy = input.ProxyPool.GetProxy();
                            }
                        }
                    }

                    var scriptGlobals = new ScriptGlobals(botData, input.Globals);
                    
                    // Set custom inputs answers
                    foreach (var answer in input.CustomInputsAnswers)
                        (scriptGlobals.input as IDictionary<string, object>).Add(answer.Key, answer.Value);

                    botData.Logger.Log($"[{DateTime.Now.ToShortTimeString()}] BOT STARTED WITH DATA {botData.Line.Data} AND PROXY {botData.Proxy}");
                    scriptState = await input.Script.RunAsync(scriptGlobals, null, token);
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
                        input.ProxyPool.ReleaseProxy(input.BotData.Proxy, true);

                    // Otherwise set it to available
                    else if (botData.Proxy.ProxyStatus == ProxyStatus.Busy)
                        input.ProxyPool.ReleaseProxy(input.BotData.Proxy, false);
                }

                if (botData.STATUS == "RETRY")
                {
                    input.Job.DataRetried++;
                    goto START;
                }
                else if (botData.STATUS == "BAN" || botData.STATUS == "ERROR")
                {
                    input.Job.DataBanned++;
                    goto START;
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

            // Wait for the start condition to be verified
            await base.Start();

            // If we're in LoliCode mode, build the Stack
            if (Config.Mode == ConfigMode.LoliCode)
                Config.Stack = new Loli2StackTranspiler().Transpile(Config.LoliCodeScript);

            // Build the C# script
            Config.CSharpScript = new Stack2CSharpTranspiler().Transpile(Config.Stack, Config.Settings);
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
            PythonCompilerOptions pco = (PythonCompilerOptions)pyengine.GetCompilerOptions();
            pco.Module &= ~ModuleOptions.Optimized;

            long index = 0;
            var workItems = DataPool.DataList.Select(line =>
            {
                var input = new MultiRunInput
                {
                    Job = this,
                    ProxyPool = proxyPool,
                    BotData = new BotData(settings.RuriLibSettings, Config.Settings, new BotLogger(), null, random,
                        new DataLine(line, wordlistType), null, ShouldUseProxies(ProxyMode, Config.Settings.ProxySettings)),
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
            taskManager = new TaskManager<MultiRunInput, CheckResult>(workItems, workFunction, Bots, DataPool.Size, Skip);
            taskManager.OnResult += DataProcessed;
            taskManager.OnStatusChanged += StatusChanged;
            taskManager.OnTaskError += PropagateTaskError;
            taskManager.OnError += PropagateError;
            taskManager.OnResult += PropagateResult;
            taskManager.OnCompleted += PropagateCompleted;

            ServicePointManager.DefaultConnectionLimit = 200;

            if (ShouldUseProxies(ProxyMode, Config.Settings.ProxySettings))
                await FetchProxiesFromSources();

            ResetStats();
            StartTimer();
            await taskManager.Start();
        }

        public override async Task Stop()
        {
            try
            {
                await taskManager?.Stop();
            }
            finally
            {
                StopTimer();
            }
        }

        public override async Task Abort()
        {
            try
            {
                await taskManager?.Abort();
            }
            finally
            {
                StopTimer();
            }
        }

        public override async Task Pause()
        {
            try
            {
                await taskManager?.Pause();
            }
            finally
            {
                StopTimer();
            }
        }

        public override async Task Resume()
        {
            await taskManager?.Resume();
            StartTimer();
        }
        #endregion

        #region Public Methods
        public async Task FetchProxiesFromSources()
        {
            // TODO: Handle exceptions properly
            var tasks = ProxySources.Select(async source => await source.GetAll());
            var results = await Task.WhenAll(tasks);

            var proxies = results.SelectMany(r => r);
            proxyPool = new ProxyPool(proxies);
        }
        #endregion

        #region Wrappers for TaskManager methods
        public async Task ChangeBots(int amount)
        {
            if (taskManager != null)
                await taskManager.SetConcurrentTasks(amount);
        }
        #endregion

        #region Propagation of TaskManager events
        private void PropagateTaskError(object sender, ErrorDetails<MultiRunInput> details)
            => OnTaskError?.Invoke(sender, details);

        private void PropagateError(object sender, Exception ex)
            => OnError?.Invoke(sender, ex);

        private void PropagateResult(object sender, ResultDetails<MultiRunInput, CheckResult> result)
            => OnResult?.Invoke(sender, result);

        private void PropagateProgress(object sender, float progress)
            => OnProgress?.Invoke(sender, progress);

        private void PropagateCompleted(object sender, EventArgs e)
            => OnCompleted?.Invoke(sender, e);
        #endregion

        #region Private Methods
        private void StartTimer()
        {
            tickTimer = new Timer(new TimerCallback(_ => OnTimerTick?.Invoke(this, EventArgs.Empty)),
                null, (int)TickInterval.TotalMilliseconds, (int)TickInterval.TotalMilliseconds);
        }

        private void StopTimer()
        {
            tickTimer?.Dispose();
        }

        private void ResetStats()
        {
            DataTested = 0;
            DataHits = 0;
            DataCustom = 0;
            DataBad = 0;
            DataRetried = 0;
            DataBanned = 0;
            DataToCheck = 0;
            DataErrors = 0;
        }

        private void StatusChanged(object sender, TaskManagerStatus status)
        {
            Status = status switch
            {
                TaskManagerStatus.Idle => JobStatus.Idle,
                TaskManagerStatus.Starting => JobStatus.Starting,
                TaskManagerStatus.Running => JobStatus.Running,
                TaskManagerStatus.Pausing => JobStatus.Pausing,
                TaskManagerStatus.Paused => JobStatus.Paused,
                TaskManagerStatus.Stopping => JobStatus.Stopping,
                TaskManagerStatus.Resuming => JobStatus.Resuming,
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
                case "FAIL": DataBad++; break;
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
                BotLogger = botData.Logger,
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
