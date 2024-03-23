using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.SignalR;
using OpenBullet2.Web.Utils;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using RuriLib.Parallelization.Models;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Notifies clients about updates on proxy check jobs.
/// </summary>
public sealed class ProxyCheckJobService : IJobService, IDisposable
{
    // Maps jobs to connections
    private readonly Dictionary<ProxyCheckJob, List<string>> _connections = new();
    private readonly IHubContext<ProxyCheckJobHub> _hub;
    private readonly JobManagerService _jobManager;
    private readonly ILogger<ProxyCheckJobService> _logger;
    private readonly EventHandler _onBotsChanged;
    private readonly EventHandler _onCompleted;
    private readonly EventHandler<Exception> _onError;
    private readonly EventHandler<ResultDetails<ProxyCheckInput, Proxy>> _onResult;

    // Event handlers
    private readonly EventHandler<JobStatus> _onStatusChanged;
    private readonly EventHandler<ErrorDetails<ProxyCheckInput>> _onTaskError;
    private readonly EventHandler _onTimerTick;

    /// <summary></summary>
    public ProxyCheckJobService(JobManagerService jobManager,
        IHubContext<ProxyCheckJobHub> hub, ILogger<ProxyCheckJobService> logger)
    {
        _jobManager = jobManager;
        _hub = hub;
        _logger = logger;

        _onStatusChanged = EventHandlers.TryAsync<JobStatus>(
            OnStatusChangedAsync,
            SendErrorAsync
        );

        _onCompleted = EventHandlers.TryAsync(
            OnCompletedAsync,
            SendErrorAsync
        );

        _onError = EventHandlers.TryAsync<Exception>(
            OnErrorAsync,
            SendErrorAsync
        );

        _onTaskError = EventHandlers.TryAsync<ErrorDetails<ProxyCheckInput>>(
            OnTaskErrorAsync,
            SendErrorAsync
        );

        _onResult = EventHandlers.TryAsync<ResultDetails<ProxyCheckInput, Proxy>>(
            OnResultAsync,
            SendErrorAsync
        );

        _onTimerTick = EventHandlers.TryAsync(
            OnTimerTickAsync,
            SendErrorAsync
        );

        _onBotsChanged = EventHandlers.TryAsync(
            OnBotsChangedAsync,
            SendErrorAsync
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public void RegisterConnection(string connectionId, int jobId)
    {
        var job = _jobManager.Jobs.FirstOrDefault(j => j.Id == jobId);

        if (job is null)
        {
            throw new EntryNotFoundException(ErrorCode.JobNotFound,
                $"Job with id {jobId} not found");
        }

        if (job is not ProxyCheckJob pcJob)
        {
            throw new BadRequestException(ErrorCode.InvalidJobType,
                $"Job with id {jobId} is not a proxy check job");
        }

        if (!_connections.ContainsKey(pcJob))
        {
            _connections[pcJob] = new List<string>();

            // Hook the event handlers to the job
            pcJob.OnStatusChanged += _onStatusChanged;
            pcJob.OnCompleted += _onCompleted;
            pcJob.OnError += _onError;
            pcJob.OnTaskError += _onTaskError;
            pcJob.OnResult += _onResult;
            pcJob.OnTimerTick += _onTimerTick;
            pcJob.OnBotsChanged += _onBotsChanged;
        }

        // Add the connection to the list
        _connections[pcJob].Add(connectionId);

        _logger.LogDebug("Registered new connection {ConnectionId} for proxy check job {JobId}",
            connectionId, jobId);
    }

    /// <inheritdoc />
    public void UnregisterConnection(string connectionId, int jobId)
    {
        var job = (ProxyCheckJob)_jobManager.Jobs.First(j => j.Id == jobId);

        _connections[job].Remove(connectionId);

        _logger.LogDebug("Unregistered connection {ConnectionId} for proxy check job {JobId}",
            connectionId, jobId);
    }

    /// <inheritdoc />
    public void Start(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Start().Forget(
            async ex =>
            {
                _logger.LogError(ex, "Could not start job {JobId}", jobId);
                await SendErrorAsync(ex);
            });
    }

    /// <inheritdoc />
    public void Stop(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Stop().Forget(
            async ex =>
            {
                _logger.LogError(ex, "Could not stop job {JobId}", jobId);
                await SendErrorAsync(ex);
            });
    }

    /// <inheritdoc />
    public void Abort(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Abort().Forget(
            async ex =>
            {
                _logger.LogError(ex, "Could not abort job {JobId}", jobId);
                await SendErrorAsync(ex);
            });
    }

    /// <inheritdoc />
    public void Pause(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Pause().Forget(
            async ex =>
            {
                _logger.LogError(ex, "Could not pause job {JobId}", jobId);
                await SendErrorAsync(ex);
            });
    }

    /// <inheritdoc />
    public void Resume(int jobId)
    {
        var job = GetJob(jobId);

        // We can only do a closure on this logger because this
        // service is a singleton!!!
        job.Resume().Forget(
            async ex =>
            {
                _logger.LogError(ex, "Could not resume job {JobId}", jobId);
                await SendErrorAsync(ex);
            });
    }

    /// <inheritdoc />
    public void SkipWait(int jobId)
    {
        var job = GetJob(jobId);
        job.SkipWait();
    }

    /// <inheritdoc />
    public void ChangeBots(int jobId, ChangeBotsMessage message)
    {
        var job = GetJob(jobId);
        job.ChangeBots(message.Desired).Forget(
            async ex =>
            {
                _logger.LogError(ex, "Could not change bots for job {JobId}", jobId);
                await SendErrorAsync(ex);
            });
    }

    // The job exists and is of the correct type, otherwise
    // we wouldn't have been able to register the connection
    private ProxyCheckJob GetJob(int jobId)
        => (ProxyCheckJob)_jobManager.Jobs.First(j => j.Id == jobId);

    private async Task OnStatusChangedAsync(object? sender, JobStatus e)
    {
        var message = new JobStatusChangedMessage { NewStatus = e };

        await NotifyClientsAsync(sender, message, JobMethods.StatusChanged);
    }

    private async Task OnCompletedAsync(object? sender, EventArgs e)
    {
        var message = new JobCompletedMessage();

        await NotifyClientsAsync(sender, message, JobMethods.Completed);
    }

    private async Task OnErrorAsync(object? sender, Exception e)
    {
        var message = new ErrorMessage { Type = e.GetType().Name, Message = e.Message, StackTrace = e.ToString() };

        await NotifyClientsAsync(sender, message, CommonMethods.Error);
    }

    private async Task OnTaskErrorAsync(object? sender, ErrorDetails<ProxyCheckInput> e)
    {
        var message = new PcjTaskErrorMessage {
            ProxyHost = e.Item.Proxy.Host, ProxyPort = e.Item.Proxy.Port, ErrorMessage = e.Exception.Message
        };

        await NotifyClientsAsync(sender, message, JobMethods.TaskError);
    }

    private async Task OnResultAsync(object? sender, ResultDetails<ProxyCheckInput, Proxy> e)
    {
        var message = new PcjNewResultMessage {
            ProxyHost = e.Result.Host,
            ProxyPort = e.Result.Port,
            WorkingStatus = e.Result.WorkingStatus,
            Ping = e.Result.Ping,
            Country = e.Result.Country
        };

        await NotifyClientsAsync(sender, message, ProxyCheckJobMethods.NewResult);
    }

    private async Task OnTimerTickAsync(object? sender, EventArgs e)
    {
        var job = (sender as ProxyCheckJob)!;

        var message = new PcjStatsMessage {
            Tested = job.Tested,
            Working = job.Working,
            NotWorking = job.NotWorking,
            CPM = job.CPM,
            Elapsed = job.Elapsed,
            Remaining = job.Remaining,
            Progress = job.Progress
        };

        await NotifyClientsAsync(sender, message, JobMethods.TimerTick);
    }

    private async Task OnBotsChangedAsync(object? sender, EventArgs e)
    {
        var job = (sender as ProxyCheckJob)!;

        var message = new BotsChangedMessage { NewValue = job.Bots };

        await NotifyClientsAsync(sender, message, JobMethods.BotsChanged);
    }

    private async Task NotifyClientsAsync(object? sender, object message,
        string method)
    {
        var job = sender as ProxyCheckJob;

        await _hub.Clients.Clients(_connections[job!]).SendAsync(
            method, message);
    }

    private Task SendErrorAsync(Exception ex)
    {
        _logger.LogError(ex, "Error while sending message to the client");
        return Task.CompletedTask;
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var job in _connections.Keys)
            {
                job.OnStatusChanged -= _onStatusChanged;
                job.OnCompleted -= _onCompleted;
                job.OnError -= _onError;
                job.OnTaskError -= _onTaskError;
                job.OnResult -= _onResult;
                job.OnBotsChanged -= _onBotsChanged;
            }
        }
    }

    /// <inheritdoc />
    ~ProxyCheckJobService()
    {
        Dispose(false);
    }
}
