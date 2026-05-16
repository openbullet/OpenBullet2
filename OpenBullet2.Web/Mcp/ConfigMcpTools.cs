using System.ComponentModel;
using System.Text.Json;
using FluentValidation;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to OpenBullet 2 configs.
/// </summary>
[McpServerToolType]
public sealed class ConfigMcpTools
{
    /// <summary>
    /// Updates the readme of a config.
    /// </summary>
    [McpServerTool(Name = "update_config_readme"),
     Description("Updates the readme of a local config and returns a minimal JSON acknowledgment.")]
    public async Task<string> UpdateConfigReadme(
        string configId,
        ConfigReadmeDto readme,
        ConfigService configService)
    {
        var config = GetConfig(configService, configId);

        if (config.IsRemote)
        {
            throw new ActionNotAllowedException(
                ErrorCode.ActionNotAllowedForRemoteConfig,
                $"Attempted to edit a remote config with id {configId}");
        }

        config.Readme = readme.MarkdownText;
        await configService.SaveAsync(config);

        return JsonSerializer.Serialize(new UpdateResult(true), JsonSerializerOptions);
    }

    /// <summary>
    /// Updates the settings of a config.
    /// </summary>
    [McpServerTool(Name = "update_config_settings"),
     Description("Updates the settings of a local config and returns a minimal JSON acknowledgment.")]
    public async Task<string> UpdateConfigSettings(
        string configId,
        ConfigSettingsDto settings,
        ConfigService configService,
        IObjectMapper mapper,
        IValidator<ConfigSettingsDto> validator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(settings, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(Environment.NewLine,
                validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct());

            throw new McpException(errorMessage);
        }

        var config = GetConfig(configService, configId);

        if (config.IsRemote)
        {
            throw new ActionNotAllowedException(
                ErrorCode.ActionNotAllowedForRemoteConfig,
                $"Attempted to edit a remote config with id {configId}");
        }

        mapper.Map(settings, config.Settings);
        await configService.SaveAsync(config);

        return JsonSerializer.Serialize(new UpdateResult(true), JsonSerializerOptions);
    }

    /// <summary>
    /// Gets the settings of a config.
    /// </summary>
    [McpServerTool(Name = "get_config_settings"),
     Description("Gets the settings of a config as a read-only JSON object.")]
    public string GetConfigSettings(
        string configId,
        ConfigService configService,
        IObjectMapper mapper)
    {
        var config = GetConfig(configService, configId);
        var dto = mapper.Map<ConfigSettingsDto>(config.Settings);

        return JsonSerializer.Serialize(dto, JsonSerializerOptions);
    }

    /// <summary>
    /// Gets the readme of a config.
    /// </summary>
    [McpServerTool(Name = "get_config_readme"),
     Description("Gets the readme of a config as a read-only JSON object with a markdownText field.")]
    public string GetConfigReadme(string configId, ConfigService configService)
    {
        var config = GetConfig(configService, configId);
        var dto = new ConfigReadmeDto { MarkdownText = config.Readme };

        return JsonSerializer.Serialize(dto, JsonSerializerOptions);
    }

    /// <summary>
    /// Lists the currently available configs.
    /// </summary>
    [McpServerTool(Name = "list_configs"),
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

    private static RuriLib.Models.Configs.Config GetConfig(ConfigService configService, string configId)
        => configService.Configs.Find(c => c.Id == configId)
            ?? throw new ApiException(ErrorCode.ConfigNotFound,
                $"Config with id {configId} was not found");

    private sealed record ConfigListItem(
        string Id,
        string Name,
        DateTime LastUpdated);

    private sealed record UpdateResult(bool Updated);
}
