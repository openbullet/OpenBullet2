using System.ComponentModel;
using ModelContextProtocol.Server;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to the OpenBullet 2 environment configuration.
/// </summary>
[McpServerToolType]
public sealed class EnvironmentMcpTools
{
    /// <summary>
    /// Returns the current Environment.ini file as read-only text.
    /// </summary>
    [McpServerTool(Name = "get_environment"),
     Description("Returns the current read-only contents of Environment.ini from the active OpenBullet 2 user data folder.")]
    public string GetOpenBulletEnvironment()
    {
        var environmentFile = Path.Combine(Globals.UserDataFolder, "Environment.ini");
        var environmentContent = File.ReadAllText(environmentFile);

        return $$"""
                 Environment.ini
                 Path: {{Path.GetFullPath(environmentFile)}}

                 {{environmentContent}}
                 """;
    }
}
