using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Services;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// SignalR hub for a multi run job.
/// </summary>
public class MultiRunJobHub : JobHub
{
    /// <summary></summary>
    public MultiRunJobHub(IAuthTokenService tokenService,
        ILogger<MultiRunJobHub> logger, MultiRunJobService jobService,
        OpenBulletSettingsService obSettingsService)
        : base(tokenService, logger, jobService, obSettingsService)
    {
    }
}
