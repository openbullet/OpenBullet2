using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Settings;
using OpenBullet2.Web.Interfaces;
using RuriLib.Services;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to OpenBullet 2 and RuriLib settings.
/// </summary>
[McpServerToolType]
public sealed class SettingsMcpTools
{
    /// <summary>
    /// Gets the current OpenBullet 2 settings.
    /// </summary>
    [McpServerTool(Name = "get_settings"),
     Description("Gets the current OpenBullet 2 settings as a read-only JSON object.")]
    public string GetSettings(
        OpenBulletSettingsService settingsService,
        IObjectMapper mapper)
    {
        var dto = mapper.Map<OpenBulletSettingsDto>(settingsService.Settings);

        return JsonSerializer.Serialize(dto, JsonSerializerOptions);
    }

    /// <summary>
    /// Gets the current RuriLib settings.
    /// </summary>
    [McpServerTool(Name = "get_rurilib_settings"),
     Description("Gets the current RuriLib settings as a read-only JSON object.")]
    public string GetRuriLibSettings(RuriLibSettingsService settingsService)
        => JsonSerializer.Serialize(settingsService.RuriLibSettings, JsonSerializerOptions);

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
