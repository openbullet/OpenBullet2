using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Services;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// SignalR hub for system performance monitoring.
/// </summary>
public class SystemPerformanceHub : AuthorizedHub
{
    private readonly PerformanceMonitorService _performanceMonitorService;

    /// <summary></summary>
    public SystemPerformanceHub(IAuthTokenService tokenService,
        OpenBulletSettingsService obSettingsService,
        PerformanceMonitorService performanceMonitorService)
        : base(tokenService, obSettingsService, false)
    {
        _performanceMonitorService = performanceMonitorService;
    }

    /// <inheritdoc />
    public async override Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        await _performanceMonitorService.RegisterConnectionAsync(Context.ConnectionId);
    }

    /// <inheritdoc />
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        await _performanceMonitorService.UnregisterConnectionAsync(Context.ConnectionId);
    }
}
