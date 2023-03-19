using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// SignalR hub for a proxy check job.
/// </summary>
public class ProxyCheckJobHub : AuthorizedHub
{
    private readonly ILogger<ProxyCheckJobHub> _logger;
    private readonly IMapper _mapper;

    /// <summary></summary>
    public ProxyCheckJobHub(IAuthTokenService tokenService,
        ILogger<ProxyCheckJobHub> logger, IMapper mapper)
        : base(tokenService, onlyAdmin: false)
    {
        _logger = logger;
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async override Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        var jobId = GetJobId();

        if (jobId is null)
        {
            throw new Exception("Please specify a job id");
        }

        _debuggerService.RegisterConnection(Context.ConnectionId, jobId);
    }

    private int? GetJobId()
    {
        var request = Context.GetHttpContext()!.Request;
        var id = request.Query["jobId"].FirstOrDefault();
        return id is null ? null : int.Parse(id);
    }
}
