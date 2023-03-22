using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Services;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// SignalR hub for a proxy check job.
/// </summary>
public class ProxyCheckJobHub : AuthorizedHub
{
    private readonly ILogger<ProxyCheckJobHub> _logger;
    private readonly IMapper _mapper;
    private readonly ProxyCheckJobService _jobService;

    /// <summary></summary>
    public ProxyCheckJobHub(IAuthTokenService tokenService,
        ILogger<ProxyCheckJobHub> logger, IMapper mapper,
        ProxyCheckJobService jobService,
        OpenBulletSettingsService obSettingsService)
        : base(tokenService, obSettingsService, onlyAdmin: false)
    {
        _logger = logger;
        _mapper = mapper;
        _jobService = jobService;
    }

    /// <inheritdoc/>
    public async override Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var jobId = GetJobId();

        if (jobId is null)
        {
            await Clients.Caller.SendAsync(
                CommonMethods.Error,
                new ErrorMessage("Please specify a job id"));

            throw new Exception("Please specify a job id");
        }

        _jobService.RegisterConnection(Context.ConnectionId, (int)jobId);
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

    private int? GetJobId()
    {
        var request = Context.GetHttpContext()!.Request;
        var id = request.Query["jobId"].FirstOrDefault();
        return id is null ? null : int.Parse(id);
    }
}
