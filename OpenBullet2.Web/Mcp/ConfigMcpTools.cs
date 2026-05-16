using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to OpenBullet 2 configs.
/// </summary>
[McpServerToolType]
public sealed class ConfigMcpTools
{
    /// <summary>
    /// Lists the currently available configs.
    /// </summary>
    [McpServerTool(Name = "list_openbullet_configs"),
     Description("Lists the currently available OpenBullet 2 configs as a read-only JSON array with id, name, and lastUpdated fields.")]
    public string ListOpenBulletConfigs(ConfigService configService)
    {
        var configs = configService.Configs
            .OrderByDescending(c => c.Metadata.LastModified)
            .Select(c => new ConfigListItem(
                c.Id,
                c.Metadata.Name,
                c.Metadata.LastModified))
            .ToList();

        return JsonSerializer.Serialize(configs, JsonSerializerOptions);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private sealed record ConfigListItem(
        string Id,
        string Name,
        DateTime LastUpdated);
}
