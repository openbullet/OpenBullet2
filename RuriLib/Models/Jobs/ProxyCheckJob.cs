using RuriLib.Functions.Http;
using RuriLib.Logging;
using RuriLib.Models.Proxies;
using RuriLib.Services;
using RuriLib.Parallelization;
using RuriLib.Parallelization.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Jobs
{
    public class ProxyCheckJob : Job
    {
        // Options
        public int Bots { get; set; } = 1;
        public int BotLimit { get; init; } = 200;
        public bool CheckOnlyUntested { get; set; } = true;
        public string Url { get; set; } = "https://google.com";
        public string SuccessKey { get; set; } = "title>Google";
        public IEnumerable<Proxy> Proxies { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
        public TimeSpan TickInterval = TimeSpan.FromMinutes(1);
        public IProxyCheckOutput ProxyOutput { get; set; }
        public IProxyGeolocationProvider GeoProvider { get; set; }

        // Getters
        public override float Progress => parallelizer?.Progress ?? -1;
        public TimeSpan Elapsed => parallelizer?.Elapsed ?? TimeSpan.Zero;
        public TimeSpan Remaining => parallelizer?.Remaining ?? System.Threading.Timeout.InfiniteTimeSpan;
        public int CPM => parallelizer?.CPM ?? 0;

        // Private fields
        private Parallelizer<ProxyCheckInput, Proxy> parallelizer;
        private Timer tickTimer;

        // Stats
        public int Total { get; set; }
        public int Tested { get; set; }
        public int Working { get; set; }
        public int NotWorking { get; set; }

        // Events
        public event EventHandler<ErrorDetails<ProxyCheckInput>> OnTaskError;
        public event EventHandler<ResultDetails<ProxyCheckInput, Proxy>> OnResult;
        public event EventHandler<Exception> OnError;
        public event EventHandler<float> OnProgress;
        public event EventHandler<JobStatus> OnStatusChanged;
        public event EventHandler OnCompleted;
        public event EventHandler OnTimerTick;

        public ProxyCheckJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger logger = null)
            : base(settings, pluginRepo, logger)
        {
            
        }

        #region Work Function
        private Func<ProxyCheckInput, CancellationToken, Task<Proxy>> workFunction = 
            new(async (input, token) =>
        {
            var options = new HttpOptions
            {
                ConnectTimeout = input.Timeout
            };

            using var handler = HttpFactory.GetProxiedHandler(input.Proxy, options, new CookieContainer());
            using var http = new HttpClient(handler) { Timeout = input.Timeout };
            
            try
            {
                // Use 2 cancellation tokens since we need to control the proxy connect timeout as well
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(input.Timeout);

                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, token);

                var sw = new Stopwatch();
                sw.Start();
                using var response = await http.GetAsync(input.Url, linkedCts.Token);
                var content = await response.Content.ReadAsStringAsync();
                sw.Stop();

                if (content.Contains(input.SuccessKey, StringComparison.InvariantCultureIgnoreCase))
                {
                    input.Proxy.WorkingStatus = ProxyWorkingStatus.Working;
                    input.Proxy.Ping = (int)sw.ElapsedMilliseconds;
                }
                else
                {
                    input.Proxy.WorkingStatus = ProxyWorkingStatus.NotWorking;
                    input.Proxy.Ping = 0;
                }
            }
            catch (OperationCanceledException)
            {
                input.Proxy.WorkingStatus = ProxyWorkingStatus.NotWorking;
                input.Proxy.Ping = (int)input.Timeout.TotalMilliseconds;
            }
            catch
            {
                input.Proxy.WorkingStatus = ProxyWorkingStatus.NotWorking;
                input.Proxy.Ping = 0;
            }
            finally
            {
                input.Proxy.LastChecked = DateTime.Now;
                http.Dispose();
            }

            // Geolocation
            if (input.GeoProvider != null)
            {
                try
                {
                    input.Proxy.Country = await input.GeoProvider.Geolocate(input.Proxy.Host);
                }
                catch
                {
                    input.Proxy.Country = "Unknown";
                }
            }

            return input.Proxy;
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

                if (Proxies == null)
                    throw new NullReferenceException("The proxy list cannot be null");

                if (ProxyOutput == null)
                    throw new NullReferenceException("The proxy check output cannot be null");

                if (Url == null)
                    throw new NullReferenceException("The url cannot be null");

                if (SuccessKey == null)
                    throw new NullReferenceException("The success key cannot be null");

                var proxies = CheckOnlyUntested
                    ? Proxies.Where(p => p.WorkingStatus == ProxyWorkingStatus.Untested)
                    : Proxies;

                // Update the stats
                Total = proxies.Count();
                Tested = proxies.Count(p => p.WorkingStatus != ProxyWorkingStatus.Untested);
                Working = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.Working);
                NotWorking = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.NotWorking);

                if (!proxies.Any())
                    throw new Exception("No proxies provided to check");

                // Wait for the start condition to be verified
                await base.Start();

                var workItems = proxies.Select(p => new ProxyCheckInput(p, Url, SuccessKey, Timeout, GeoProvider));
                parallelizer = ParallelizerFactory<ProxyCheckInput, Proxy>
                    .Create(settings.RuriLibSettings.GeneralSettings.ParallelizerType, workItems,
                    workFunction, Bots, Proxies.Count(), 0, BotLimit);

                parallelizer.NewResult += UpdateProxy;
                parallelizer.ProgressChanged += PropagateProgress;
                parallelizer.StatusChanged += StatusChanged;
                parallelizer.TaskError += PropagateTaskError;
                parallelizer.Error += PropagateError;
                parallelizer.NewResult += PropagateResult;
                parallelizer.Completed += PropagateCompleted;

                ResetStats();
                StartTimer();
                logger?.LogInfo(Id, "All set, starting the execution");
                await parallelizer.Start();
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
                await parallelizer?.Stop();
            }
            finally
            {
                StopTimer();
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
                StopTimer();
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
                StopTimer();
                logger?.LogInfo(Id, "Execution paused");
            }
        }

        public override async Task Resume()
        {
            await parallelizer?.Resume();
            StartTimer();
            logger?.LogInfo(Id, "Execution resumed");
        }
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
        private void PropagateTaskError(object _, ErrorDetails<ProxyCheckInput> details)
        {
            OnTaskError?.Invoke(this, details);
            logger?.LogException(Id, details.Exception);
        }

        private void PropagateError(object _, Exception ex)
        {
            OnError?.Invoke(this, ex);
            logger?.LogException(Id, ex);
        }

        private void PropagateResult(object _, ResultDetails<ProxyCheckInput, Proxy> result)
        {
            OnResult?.Invoke(this, result);
            // We're not logging results to the IJobLogger because they could arrive at a very high rate
            // and not be very useful, we're mostly interested in errors here.
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
            Tested = 0;
            Working = 0;
            NotWorking = 0;
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

        private void UpdateProxy(object sender, ResultDetails<ProxyCheckInput, Proxy> details)
        {
            var proxy = details.Result;

            if (proxy.WorkingStatus == ProxyWorkingStatus.Working) Working++;
            else NotWorking++;

            Tested++;

            // This is fire and forget
            _ = ProxyOutput.Store(proxy);
        }
        #endregion
    }

    public struct ProxyCheckInput
    {
        public Proxy Proxy { get; set; }
        public string Url { get; set; }
        public string SuccessKey { get; set; }
        public TimeSpan Timeout { get; set; }
        public IProxyGeolocationProvider GeoProvider { get; set; }

        public ProxyCheckInput(Proxy proxy, string url, string successKey,
            TimeSpan timeout, IProxyGeolocationProvider geoProvider)
        {
            Proxy = proxy;
            Url = url;
            SuccessKey = successKey;
            Timeout = timeout;
            GeoProvider = geoProvider;
        }
    }
}
