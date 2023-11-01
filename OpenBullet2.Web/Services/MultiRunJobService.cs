using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.SignalR;
using OpenBullet2.Web.Utils;
using RuriLib.Models.Hits;
using RuriLib.Models.Jobs;
using RuriLib.Parallelization.Models;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Notifies clients about updates on multi run jobs.
/// </summary>
public class MultiRunJobService : IJobService, IDisposable
{
    private readonly JobManagerService _jobManager;
    private readonly IHubContext<MultiRunJobHub> _hub;
    private readonly ILogger<MultiRunJobService> _logger;

    // Maps jobs to connections
    private readonly Dictionary<MultiRunJob, List<string>> _connections = new();

    // Event handlers
    private readonly EventHandler<JobStatus> _onStatusChanged;
    private readonly EventHandler _onCompleted;
    private readonly EventHandler<Exception> _onError;
    private readonly EventHandler<ErrorDetails<MultiRunInput>> _onTaskError;
    private readonly EventHandler<ResultDetails<MultiRunInput, CheckResult>> _onResult;
    private readonly EventHandler _onTimerTick;
    private readonly EventHandler<Hit> _onHit;
    private readonly EventHandler _onBotsChanged;

    /// <summary></summary>
    public MultiRunJobService(JobManagerService jobManager,
        IHubContext<MultiRunJobHub> hub, ILogger<MultiRunJobService> logger)
    {
        _jobManager = jobManager;
        _hub = hub;
        _logger = logger;

        _onStatusChanged = EventHandlers.TryAsync<JobStatus>(
            OnStatusChanged,
            SendError
        );

        _onCompleted = EventHandlers.TryAsync(
            OnCompleted,
            SendError
        );

        _onError = EventHandlers.TryAsync<Exception>(
            OnError,
            SendError
        );

        _onTaskError = EventHandlers.TryAsync<ErrorDetails<MultiRunInput>>(
            OnTaskError,
            SendError
        );

        _onResult = EventHandlers.TryAsync<ResultDetails<MultiRunInput, CheckResult>>(
            OnResult,
            SendError
        );

        _onTimerTick = EventHandlers.TryAsync(
            OnTimerTick,
            SendError
        );

        _onHit = EventHandlers.TryAsync<Hit>(
            OnHit,
            SendError
        );

        _onBotsChanged = EventHandlers.TryAsync(
            OnBotsChanged,
            SendError
        );
    }

    /// <inheritdoc/>
    public void RegisterConnection(string connectionId, int jobId)
    {
        var job = _jobManager.Jobs.FirstOrDefault(j => j.Id == jobId);

        if (job is null)
        {
            throw new EntryNotFoundException(ErrorCode.JOB_NOT_FOUND,
                $"Job with id {jobId} not found");
        }

        if (job is not MultiRunJob mrJob)
        {
            throw new BadRequestException(ErrorCode.INVALID_JOB_TYPE,
                $"Job with id {jobId} is not a multi run job");
        }

        if (!_connections.ContainsKey(mrJob))
        {
            _connections[mrJob] = new();

            // Hook the event handlers to the job
            mrJob.OnStatusChanged += _onStatusChanged;
            mrJob.OnCompleted += _onCompleted;
            mrJob.OnError += _onError;
            mrJob.OnTaskError += _onTaskError;
            mrJob.OnResult += _onResult;
            mrJob.OnTimerTick += _onTimerTick;
            mrJob.OnHit += _onHit;
            mrJob.OnBotsChanged += _onBotsChanged;
        }

        // Add the connection to the list
        _connections[mrJob].Add(connectionId);

        _logger.LogDebug($"Registered new connection {connectionId} for multi run job {jobId}");
    }

    /// <inheritdoc/>
    public void UnregisterConnection(string connectionId, int jobId)
    {
        var job = (MultiRunJob)_jobManager.Jobs.First(j => j.Id == jobId);

        _connections[job].Remove(connectionId);

        _logger.LogDebug($"Unregistered connection {connectionId} for multi run job {jobId}");
    }

    /// <inheritdoc/>
    public void Start(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Start().Forget(
            async ex => {
                _logger.LogError(ex, $"Could not start job {jobId}");
                await SendError(ex);
            });
    }

    /// <inheritdoc/>
    public void Stop(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Stop().Forget(
            async ex => {
                _logger.LogError(ex, $"Could not stop job {jobId}");
                await SendError(ex);
            });
    }

    /// <inheritdoc/>
    public void Abort(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Abort().Forget(
            async ex => {
                _logger.LogError(ex, $"Could not abort job {jobId}");
                await SendError(ex);
            });
    }

    /// <inheritdoc/>
    public void Pause(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Pause().Forget(
            async ex => {
                _logger.LogError(ex, $"Could not pause job {jobId}");
                await SendError(ex);
            });
    }

    /// <inheritdoc/>
    public void Resume(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Resume().Forget(
            async ex => {
                _logger.LogError(ex, $"Could not resume job {jobId}");
                await SendError(ex);
            });
    }

    /// <inheritdoc/>
    public void SkipWait(int jobId)
    {
        var job = GetJob(jobId);
        job.SkipWait();
    }
    
    /// <inheritdoc/>
    public void ChangeBots(int jobId, ChangeBotsMessage message)
    {
        var job = GetJob(jobId);
        job.ChangeBots(message.Desired).Forget(
            async ex =>
            {
                _logger.LogError(ex, $"Could not change bots for job {jobId}");
                await SendError(ex);
            });
    }

    // The job exists and is of the correct type, otherwise
    // we wouldn't have been able to register the connection
    private MultiRunJob GetJob(int jobId)
        => (MultiRunJob)_jobManager.Jobs.First(j => j.Id == jobId);

    private async Task OnStatusChanged(object? sender, JobStatus e)
    {
        var message = new JobStatusChangedMessage
        {
            NewStatus = e
        };

        await NotifyClients(sender, message, JobMethods.StatusChanged);
    }

    private async Task OnCompleted(object? sender, EventArgs e)
    {
        var message = new JobCompletedMessage();

        await NotifyClients(sender, message, JobMethods.Completed);
    }

    private async Task OnError(object? sender, Exception e)
    {
        var message = new ErrorMessage
        {
            Type = e.GetType().Name,
            Message = e.Message,
            StackTrace = e.ToString()
        };

        await NotifyClients(sender, message, CommonMethods.Error);
    }

    private async Task OnTaskError(object? sender, ErrorDetails<MultiRunInput> e)
    {
        var message = new MRJTaskErrorMessage
        {
            DataLine = e.Item.BotData.Line.Data,
            Proxy = e.Item.BotData.Proxy is null ? null : new MRJProxy
            {
                Host = e.Item.BotData.Proxy.Host,
                Port = e.Item.BotData.Proxy.Port
            },
            ErrorMessage = e.Exception.Message
        };

        await NotifyClients(sender, message, JobMethods.TaskError);
    }

    private async Task OnResult(object? sender, ResultDetails<MultiRunInput, CheckResult> e)
    {
        var message = new MRJNewResultMessage
        {
            DataLine = e.Item.BotData.Line.Data,
            Proxy = e.Item.BotData.Proxy is null ? null : new MRJProxy
            {
                Host = e.Item.BotData.Proxy.Host,
                Port = e.Item.BotData.Proxy.Port
            },
            Status = e.Result.BotData.STATUS
        };

        await NotifyClients(sender, message, MultiRunJobMethods.NewResult);
    }

    private async Task OnTimerTick(object? sender, EventArgs e)
    {
        var job = (sender as MultiRunJob)!;

        var message = new MRJStatsMessage
        {
            DataStats = new MRJDataStatsDto
            {
                Hits = job.DataHits,
                Custom = job.DataCustom,
                Fails = job.DataFails,
                Invalid = job.DataInvalid,
                Retried = job.DataRetried,
                Banned = job.DataBanned,
                Errors = job.DataErrors,
                ToCheck = job.DataToCheck,
                Total = job.DataPool.Size,
                Tested = job.DataTested
            },
            ProxyStats = new MRJProxyStatsDto
            {
                Total = job.ProxiesTotal,
                Alive = job.ProxiesAlive,
                Bad = job.ProxiesBad,
                Banned = job.ProxiesBanned
            },
            CPM = job.CPM,
            CaptchaCredit = job.CaptchaCredit,
            Elapsed = job.Elapsed,
            Remaining = job.Remaining,
            Progress = job.Progress
        };

        await NotifyClients(sender, message, JobMethods.TimerTick);
    }

    private async Task OnHit(object? sender, Hit e)
    {
        var message = new MRJNewHitMessage
        {
            DataLine = e.DataString,
            Proxy = e.Proxy is null ? null : new MRJProxy
            {
                Host = e.Proxy.Host,
                Port = e.Proxy.Port
            },
            Date = e.Date,
            Type = e.Type,
            CaptureString = e.CapturedDataString
        };

        await NotifyClients(sender, message, MultiRunJobMethods.NewHit);
    }

    private async Task OnBotsChanged(object? sender, EventArgs e)
    {
        var job = (sender as MultiRunJob)!;

        var message = new BotsChangedMessage
        {
            NewValue = job.Bots
        };

        await NotifyClients(sender, message, JobMethods.BotsChanged);
    }

    private async Task NotifyClients(object? sender, object message,
        string method)
    {
        var job = (sender as MultiRunJob);

        await _hub.Clients.Clients(_connections[job!]).SendAsync(
            method, message);
    }

    private Task SendError(Exception ex)
    {
        _logger.LogError(ex, "Error while sending message to the client");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var job in _connections.Keys)
        {
            job.OnStatusChanged -= _onStatusChanged;
            job.OnCompleted -= _onCompleted;
            job.OnError -= _onError;
            job.OnTaskError -= _onTaskError;
            job.OnResult -= _onResult;
            job.OnHit -= _onHit;
            job.OnBotsChanged -= _onBotsChanged;
        }
    }
}
