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

namespace RuriLib.Models.Jobs;

/// <summary>
/// Represents a job that checks proxy connectivity and status.
/// </summary>
public class ProxyCheckJob : Job
{
    // Options
    /// <summary>
    /// Gets or sets the number of worker bots.
    /// </summary>
    public int Bots { get; set; } = 1;

    /// <summary>
    /// Gets the maximum allowed number of worker bots.
    /// </summary>
    public int BotLimit { get; init; } = 200;

    /// <summary>
    /// Gets or sets a value indicating whether only untested proxies should be checked.
    /// </summary>
    public bool CheckOnlyUntested { get; set; } = true;

    /// <summary>
    /// Gets or sets the URL used for proxy verification.
    /// </summary>
    public string? Url { get; set; } = "https://google.com";

    /// <summary>
    /// Gets or sets the success marker expected in the response.
    /// </summary>
    public string? SuccessKey { get; set; } = "title>Google";

    /// <summary>
    /// Gets or sets the proxies to test.
    /// </summary>
    public IEnumerable<Proxy>? Proxies { get; set; }

    /// <summary>
    /// Gets or sets the timeout for each proxy check.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets a value indicating whether to use a proxy judge.
    /// </summary>
    public bool UseProxyJudge { get; set; } = true;

    /// <summary>
    /// Gets or sets the proxy judge used to determine proxy quality.
    /// </summary>
    public IProxyJudge ProxyJudge { get; set; } = new AzenvProxyJudge();

    /// <summary>
    /// Gets or sets the timer tick interval.
    /// </summary>
    public TimeSpan TickInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the proxy output sink.
    /// </summary>
    public IProxyCheckOutput? ProxyOutput { get; set; }

    /// <summary>
    /// Gets or sets the optional proxy geolocation provider.
    /// </summary>
    public IProxyGeolocationProvider? GeoProvider { get; set; }

    // Getters
    /// <inheritdoc />
    public override float Progress => parallelizer?.Progress ?? -1;
    /// <inheritdoc />
    public override TimeSpan Elapsed => parallelizer?.Elapsed ?? TimeSpan.Zero;
    /// <inheritdoc />
    public override TimeSpan Remaining => parallelizer?.Remaining ?? System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Gets the current checks per minute.
    /// </summary>
    public int CPM => parallelizer?.CPM ?? 0;

    // Private fields
    private bool disposed;
    private Parallelizer<ProxyCheckInput, Proxy>? parallelizer;
    private Timer? tickTimer;
    private CancellationTokenSource? startCts;
    private JobLastRunOutcome pendingLastRunOutcome = JobLastRunOutcome.None;

    // Stats
    /// <summary>
    /// Gets or sets the total number of proxies to process.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Gets or sets the number of tested proxies.
    /// </summary>
    public int Tested { get; set; }

    /// <summary>
    /// Gets or sets the number of working proxies.
    /// </summary>
    public int Working { get; set; }

    /// <summary>
    /// Gets or sets the number of non-working proxies.
    /// </summary>
    public int NotWorking { get; set; }

    // Events
    /// <summary>
    /// Raised when a worker task fails.
    /// </summary>
    public event EventHandler<ErrorDetails<ProxyCheckInput>>? OnTaskError;

    /// <summary>
    /// Raised when a proxy-check result is produced.
    /// </summary>
    public event EventHandler<ResultDetails<ProxyCheckInput, Proxy>>? OnResult;

    /// <summary>
    /// Raised when the job encounters an error.
    /// </summary>
    public event EventHandler<Exception>? OnError;

    /// <summary>
    /// Raised when job progress changes.
    /// </summary>
    public event EventHandler<float>? OnProgress;

    /// <summary>
    /// Raised when the job status changes.
    /// </summary>
    public event EventHandler<JobStatus>? OnStatusChanged;

    /// <summary>
    /// Raised when the bot count changes.
    /// </summary>
    public event EventHandler? OnBotsChanged;

    /// <summary>
    /// Raised when the job completes.
    /// </summary>
    public event EventHandler? OnCompleted;

    /// <summary>
    /// Raised on each timer tick.
    /// </summary>
    public event EventHandler? OnTimerTick;

    /// <summary>
    /// Creates a proxy-check job.
    /// </summary>
    /// <param name="settings">The RuriLib settings service.</param>
    /// <param name="pluginRepo">The plugin repository.</param>
    /// <param name="logger">The optional job logger.</param>
    public ProxyCheckJob(RuriLibSettingsService settings, PluginRepository pluginRepo, IJobLogger? logger = null)
        : base(settings, pluginRepo, logger)
    {
    }

    #region Work Function
    private readonly Func<ProxyCheckInput, CancellationToken, Task<Proxy>> workFunction =
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
                var content = await response.Content.ReadAsStringAsync(linkedCts.Token);
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
            }

            if (input.UseProxyJudge)
            {
                input.Proxy.Quality = await input.ProxyJudge.DetermineQualityAsync(
                    http, input.JudgeUrls, input.Timeout, token);
            }

            // Geolocation
            if (input.GeoProvider is not null)
            {
                try
                {
                    input.Proxy.Country = await input.GeoProvider.GeolocateAsync(input.Proxy.Host);
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
    /// <inheritdoc />
    public override async Task Start(CancellationToken cancellationToken = default)
    {
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

            if (Proxies is null)
                throw new NullReferenceException("The proxy list cannot be null");

            if (ProxyOutput is null)
                throw new NullReferenceException("The proxy check output cannot be null");

            if (Url is null)
                throw new NullReferenceException("The url cannot be null");

            if (SuccessKey is null)
                throw new NullReferenceException("The success key cannot be null");

            var proxies = (CheckOnlyUntested
                ? Proxies.Where(p => p.WorkingStatus == ProxyWorkingStatus.Untested)
                : Proxies).ToList();

            // Update the stats
            Total = proxies.Count;
            Tested = proxies.Count(p => p.WorkingStatus != ProxyWorkingStatus.Untested);
            Working = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.Working);
            NotWorking = proxies.Count(p => p.WorkingStatus == ProxyWorkingStatus.NotWorking);

            if (proxies.Count == 0)
                throw new Exception("No proxies provided to check");

            Status = JobStatus.Waiting;
            OnStatusChanged?.Invoke(this, Status);

            // Wait for the start condition to be verified
            await base.Start(linkedCts.Token).ConfigureAwait(false);

            Status = JobStatus.Starting;
            OnStatusChanged?.Invoke(this, Status);

            var judgeUrls = settings.RuriLibSettings.GeneralSettings.ProxyJudgeUrls
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            var workItems = proxies.Select(p =>
                new ProxyCheckInput(p, Url, SuccessKey, Timeout, GeoProvider, UseProxyJudge,
                    judgeUrls, ProxyJudge));
            parallelizer = ParallelizerFactory<ProxyCheckInput, Proxy>
                .Create(settings.RuriLibSettings.GeneralSettings.ParallelizerType, workItems,
                workFunction, Bots, proxies.Count, 0, BotLimit);

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
            await parallelizer.Start().ConfigureAwait(false);
        }
        catch (TaskCanceledException)
        {
            if (LastRunOutcome == JobLastRunOutcome.None && pendingLastRunOutcome != JobLastRunOutcome.None)
            {
                LastRunOutcome = pendingLastRunOutcome;
            }
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
            if (Status is JobStatus.Starting or JobStatus.Waiting)
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
                LastRunOutcome = JobLastRunOutcome.Stopped;
            }

            StopTimer();
            logger?.LogInfo(Id, "Execution stopped");
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

            StopTimer();
            logger?.LogInfo(Id, "Execution aborted");
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
            StopTimer();
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

        StartTimer();
        logger?.LogInfo(Id, "Execution resumed");
    }
    #endregion

    #region Wrappers for TaskManager methods
    /// <summary>
    /// Changes the number of worker bots used by the job.
    /// </summary>
    /// <param name="amount">The new degree of parallelism.</param>
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

    #region Propagation of TaskManager events
    private void PropagateTaskError(object? _, ErrorDetails<ProxyCheckInput> details)
    {
        OnTaskError?.Invoke(this, details);
        logger?.LogException(Id, details.Exception);
    }

    private void PropagateError(object? _, Exception ex)
    {
        OnError?.Invoke(this, ex);
        logger?.LogException(Id, ex);
    }

    private void PropagateResult(object? _, ResultDetails<ProxyCheckInput, Proxy> result)
    {
        OnResult?.Invoke(this, result);
        // We're not logging results to the IJobLogger because they could arrive at a very high rate
        // and not be very useful, we're mostly interested in errors here.
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

        StopTimer();
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
        tickTimer = null;
    }

    private void ResetStats()
    {
        Tested = 0;
        Working = 0;
        NotWorking = 0;
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

    private void UpdateProxy(object? sender, ResultDetails<ProxyCheckInput, Proxy> details)
    {
        var proxy = details.Result;

        if (proxy.WorkingStatus == ProxyWorkingStatus.Working) Working++;
        else NotWorking++;

        Tested++;

        // This is fire and forget
        _ = ProxyOutput?.StoreAsync(proxy);
    }

    private void ResetForNewRun()
    {
        StopTimer();
        DisposeParallelizer();
    }

    private void DisposeParallelizer()
    {
        if (parallelizer is null)
        {
            return;
        }

        try
        {
            parallelizer.Dispose();
        }
        catch
        {
            // ignored
        }
        finally
        {
            parallelizer = null;
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposed || !disposing)
        {
            return;
        }

        StopTimer();
        startCts?.Dispose();
        startCts = null;
        DisposeParallelizer();

        if (ProxyOutput is IDisposable proxyOutput)
        {
            try
            {
                proxyOutput.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        ProxyOutput = null;

        if (GeoProvider is IDisposable geoProvider)
        {
            try
            {
                geoProvider.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        GeoProvider = null;
        disposed = true;
        base.Dispose(disposing);
    }

    #endregion
}

/// <summary>
/// Represents the input required to check a single proxy.
/// </summary>
public struct ProxyCheckInput
{
    /// <summary>
    /// Gets or sets the proxy to check.
    /// </summary>
    public Proxy Proxy { get; set; }

    /// <summary>
    /// Gets or sets the verification URL.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the success marker expected in the response.
    /// </summary>
    public string SuccessKey { get; set; }

    /// <summary>
    /// Gets or sets the timeout for the check.
    /// </summary>
    public TimeSpan Timeout { get; set; }

    /// <summary>
    /// Gets or sets the optional geolocation provider.
    /// </summary>
    public IProxyGeolocationProvider? GeoProvider { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use a proxy judge.
    /// </summary>
    public bool UseProxyJudge { get; set; }

    /// <summary>
    /// Gets or sets the list of proxy judge URLs to try in order.
    /// </summary>
    public IReadOnlyList<string> JudgeUrls { get; set; }

    /// <summary>
    /// Gets or sets the proxy judge implementation.
    /// </summary>
    public IProxyJudge ProxyJudge { get; set; }

    /// <summary>
    /// Creates a proxy-check input payload.
    /// </summary>
    /// <param name="proxy">The proxy to check.</param>
    /// <param name="url">The verification URL.</param>
    /// <param name="successKey">The success marker expected in the response.</param>
    /// <param name="timeout">The timeout for the check.</param>
    /// <param name="geoProvider">The optional geolocation provider.</param>
    /// <param name="useProxyJudge">Whether proxy quality should be determined through a proxy judge.</param>
    /// <param name="judgeUrls">The judge URLs to try in order.</param>
    /// <param name="proxyJudge">The proxy judge implementation.</param>
    public ProxyCheckInput(Proxy proxy, string url, string successKey,
        TimeSpan timeout, IProxyGeolocationProvider? geoProvider, bool useProxyJudge,
        IReadOnlyList<string> judgeUrls, IProxyJudge proxyJudge)
    {
        Proxy = proxy;
        Url = url;
        SuccessKey = successKey;
        Timeout = timeout;
        GeoProvider = geoProvider;
        UseProxyJudge = useProxyJudge;
        JudgeUrls = judgeUrls;
        ProxyJudge = proxyJudge;
    }
}
