using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Services;
using System.Text.Json;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// SignalR hub for the config debugger.
/// </summary>
public class ConfigDebuggerHub : AuthorizedHub
{
    private readonly ConfigService _configService;
    private readonly ConfigDebuggerService _debuggerService;
    private readonly IAuthTokenService _tokenService;

    /// <summary></summary>
    public ConfigDebuggerHub(ConfigService configService,
        ConfigDebuggerService debuggerService, IAuthTokenService tokenService)
        : base(tokenService, onlyAdmin: true)
    {
        _configService = configService;
        _debuggerService = debuggerService;
        _tokenService = tokenService;
    }

    public async Task Test(object dto)
    {
        var json = JsonSerializer.Serialize(dto, Globals.JsonOptions);
        Console.WriteLine(json);
    }
}
