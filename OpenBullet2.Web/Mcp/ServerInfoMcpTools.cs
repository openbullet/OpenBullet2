using System.ComponentModel;
using ModelContextProtocol.Server;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to OpenBullet 2 server state.
/// </summary>
[McpServerToolType]
public sealed class ServerInfoMcpTools
{
    /// <summary>
    /// Returns a small read-only summary of the current server state.
    /// </summary>
    [McpServerTool(Name = "get_server_info"),
     Description("Returns basic read-only information about the running OpenBullet 2 web server.")]
    public string GetOpenBulletServerInfo(
        IUpdateService updateService,
        OpenBulletSettingsService settingsService,
        UserDataDirectoryProvider userDataDirectory)
    {
        var uptime = DateTime.UtcNow - Globals.StartTime;
        var userDataFolder = Path.GetFullPath(userDataDirectory.RootPath);

        return $$"""
                 OpenBullet 2 server info
                 Version: {{updateService.CurrentVersion}}
                 StartedAtUtc: {{Globals.StartTime:O}}
                 Uptime: {{uptime:c}}
                 RequireAdminLogin: {{settingsService.Settings.SecuritySettings.RequireAdminLogin}}
                 UserDataFolder: {{userDataFolder}}
                 """;
    }
}
