using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.SignalR;
using OpenBullet2.Web.Utils;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using RuriLib.Parallelization.Models;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Notifies clients about updates on proxy check jobs.
/// </summary>
public class ProxyCheckJobService : IDisposable
{
    private readonly JobManagerService _jobManager;
    private readonly IHubContext<ProxyCheckJobHub> _hub;
    private readonly ILogger<ProxyCheckJobService> _logger;

    // Maps jobs to connections
    private readonly Dictionary<ProxyCheckJob, List<string>> _connections = new();

    // Event handlers
    private readonly EventHandler<JobStatus> _onStatusChanged;
    private readonly EventHandler _onCompleted;
    private readonly EventHandler<Exception> _onError;
    private readonly EventHandler<ErrorDetails<ProxyCheckInput>> _onTaskError;
    private readonly EventHandler<ResultDetails<ProxyCheckInput, Proxy>> _onResult;
    private readonly EventHandler _onTimerTick;

    /// <summary></summary>
    public ProxyCheckJobService(JobManagerService jobManager,
        IHubContext<ProxyCheckJobHub> hub, ILogger<ProxyCheckJobService> logger)
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

        _onTaskError = EventHandlers.TryAsync<ErrorDetails<ProxyCheckInput>>(
            OnTaskError,
            SendError
        );

        _onResult = EventHandlers.TryAsync<ResultDetails<ProxyCheckInput, Proxy>>(
            OnResult,
            SendError
        );

        _onTimerTick = EventHandlers.TryAsync(
            OnTimerTick,
            SendError
        );
    }

    /// <summary>
    /// Registers a new connection, a.k.a. an interactive job session started
    /// by a given client for a given config.
    /// </summary>
    public void RegisterConnection(string connectionId, int jobId)
    {
        var job = _jobManager.Jobs.FirstOrDefault(j => j.Id == jobId);

        if (job is null)
        {
            throw new EntryNotFoundException(ErrorCode.JOB_NOT_FOUND,
                $"Job with id {jobId} not found");
        }

        if (job is not ProxyCheckJob pcJob)
        {
            throw new BadRequestException(ErrorCode.INVALID_JOB_TYPE,
                $"Job with id {jobId} is not a proxy check job");
        }

        if (!_connections.ContainsKey(pcJob))
        {
            _connections[pcJob] = new();

            // Hook the event handlers to the job
            pcJob.OnStatusChanged += _onStatusChanged;
            pcJob.OnCompleted += _onCompleted;
            pcJob.OnError += _onError;
            pcJob.OnTaskError += _onTaskError;
            pcJob.OnResult += _onResult;
            pcJob.OnTimerTick += _onTimerTick;
        }

        // Add the connection to the list
        _connections[pcJob].Add(connectionId);

        _logger.LogDebug($"Registered new connection {connectionId} for proxy check job {jobId}");
    }

    /// <summary>
    /// Unregisters an existing connection.
    /// </summary>
    public void UnregisterConnection(string connectionId, int jobId)
    {
        var job = (ProxyCheckJob)_jobManager.Jobs.First(j => j.Id == jobId);

        _connections[job].Remove(connectionId);

        _logger.LogDebug($"Unregistered connection {connectionId} for proxy check job {jobId}");
    }

    private async Task OnStatusChanged(object? sender, JobStatus e)
    {
        var message = new JobStatusChangedMessage
        {
            NewStatus = e
        };

        await NotifyClients(sender, message);
    }

    private async Task OnCompleted(object? sender, EventArgs e)
    {
        var message = new JobCompletedMessage();

        await NotifyClients(sender, message);
    }

    private async Task OnError(object? sender, Exception e)
    {
        var message = new ErrorMessage
        {
            Type = e.GetType().Name,
            Message = e.Message,
            StackTrace = e.ToString()
        };

        await NotifyClients(sender, message);
    }
    
    private async Task OnTaskError(object? sender, ErrorDetails<ProxyCheckInput> e)
    {
        var message = new PCJobTaskErrorMessage
        {
            ProxyHost = e.Item.Proxy.Host,
            ProxyPort = e.Item.Proxy.Port,
            ErrorMessage = e.Exception.Message
        };

        await NotifyClients(sender, message);
    }

    private async Task OnResult(object? sender, ResultDetails<ProxyCheckInput, Proxy> e)
    {
        var message = new PCJobNewResultMessage
        {
            ProxyHost = e.Result.Host,
            ProxyPort = e.Result.Port,
            WorkingStatus = e.Result.WorkingStatus,
            Ping = e.Result.Ping,
            Country = e.Result.Country
        };

        await NotifyClients(sender, message);
    }

    private async Task OnTimerTick(object? sender, EventArgs e)
    {
        var job = (sender as ProxyCheckJob)!;

        var message = new PCJobStatsMessage
        {
            Tested = job.Tested,
            Working = job.Working,
            NotWorking = job.NotWorking,
            CPM = job.CPM,
            Elapsed = job.Elapsed,
            Remaining = job.Remaining,
            Progress = job.Progress
        };

        await NotifyClients(sender, message);
    }

    private async Task NotifyClients(object? sender, object message)
    {
        var job = (sender as ProxyCheckJob);

        await _hub.Clients.Clients(_connections[job!]).SendAsync(
            ProxyCheckJobMethods.StatusChanged, message);
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
        }
    }
}
