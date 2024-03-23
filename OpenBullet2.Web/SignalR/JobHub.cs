using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// SignalR hub for a generic job.
/// </summary>
public abstract class JobHub : AuthorizedHub
{
    private readonly IJobService _jobService;

    /// <summary></summary>
    protected JobHub(IAuthTokenService tokenService,
        ILogger logger, IJobService jobService,
        OpenBulletSettingsService obSettingsService)
        : base(tokenService, obSettingsService, false)
    {
        _jobService = jobService;
    }

    /// <inheritdoc />
    public async override Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var jobId = GetJobId();

        if (jobId is null)
        {
            await Clients.Caller.SendAsync(
                CommonMethods.Error,
                new ErrorMessage("Please specify a job id"));

            throw new ApiException(ErrorCode.MissingJobId, "Please specify a job id");
        }

        _jobService.RegisterConnection(Context.ConnectionId, (int)jobId);
    }

    /// <inheritdoc />
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);

        var jobId = GetJobId();

        _jobService.UnregisterConnection(Context.ConnectionId, (int)jobId!);
    }

    /// <summary>
    /// Start a job.
    /// </summary>
    [HubMethodName("start")]
    public void Start() => _jobService.Start((int)GetJobId()!);

    /// <summary>
    /// Stop a job.
    /// </summary>
    [HubMethodName("stop")]
    public void Stop() => _jobService.Stop((int)GetJobId()!);

    /// <summary>
    /// Abort a job.
    /// </summary>
    [HubMethodName("abort")]
    public void Abort() => _jobService.Abort((int)GetJobId()!);

    /// <summary>
    /// Pause a job.
    /// </summary>
    [HubMethodName("pause")]
    public void Pause() => _jobService.Pause((int)GetJobId()!);

    /// <summary>
    /// Resume a job.
    /// </summary>
    [HubMethodName("resume")]
    public void Resume() => _jobService.Resume((int)GetJobId()!);

    /// <summary>
    /// Skip the wait for a job.
    /// </summary>
    [HubMethodName("skipWait")]
    public void SkipWait() => _jobService.SkipWait((int)GetJobId()!);

    /// <summary>
    /// Change the number of bots.
    /// </summary>
    [HubMethodName("changeBots")]
    public void ChangeBots(ChangeBotsMessage message) =>
        _jobService.ChangeBots((int)GetJobId()!, message);

    /// <summary>
    /// Gets the job id provided by the user at connection setup.
    /// </summary>
    private int? GetJobId()
    {
        var request = Context.GetHttpContext()!.Request;
        var id = request.Query["jobId"].FirstOrDefault();
        return id is null ? null : int.Parse(id);
    }
}
