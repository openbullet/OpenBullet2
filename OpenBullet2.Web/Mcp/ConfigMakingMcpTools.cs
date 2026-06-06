using System.ComponentModel;
using System.Text.Json;
using FluentValidation;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Config;
using OpenBullet2.Web.Dtos.Config.Settings;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using RuriLib.Exceptions;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Configs;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to config making.
/// </summary>
[McpServerToolType]
public sealed class ConfigMakingMcpTools
{
    /// <summary>
    /// Converts a config's current LoliCode to the emitted C# scripts.
    /// </summary>
    [McpServerTool(Name = "convert_lolicode_to_csharp"),
     Description("Converts a config's current main and startup LoliCode scripts to the emitted C# scripts as a read-only JSON object.")]
    public string ConvertLoliCodeToCSharp(
        string configId,
        ConfigService configService)
    {
        var config = GetConfig(configService, configId);
        var mainScript = config.Mode == ConfigMode.Stack
            ? Stack2LoliTranspiler.Transpile(config.Stack)
            : config.LoliCodeScript;

        try
        {
            var result = new ConvertLoliCodeResult(
                true,
                Loli2CSharpTranspiler.Transpile(mainScript, config.Settings),
                Loli2CSharpTranspiler.Transpile(config.StartupLoliCodeScript, config.Settings));

            return JsonSerializer.Serialize(result, JsonSerializerOptions);
        }
        catch (LoliCodeParsingException ex)
        {
            return JsonSerializer.Serialize(new ConvertLoliCodeResult(false, string.Empty, string.Empty, ex.Message),
                JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new ConvertLoliCodeResult(false, string.Empty, string.Empty, ex.Message),
                JsonSerializerOptions);
        }
    }

    /// <summary>
    /// Gets the LoliCode making data of a config.
    /// </summary>
    [McpServerTool(Name = "get_config_lolicode"),
     Description("Gets a config's LoliCode making data as a read-only JSON object with the main script, startup script, and custom usings.")]
    public string GetConfigLoliCode(
        string configId,
        ConfigService configService)
    {
        var config = GetConfig(configService, configId);
        var result = new ConfigLoliCodeResult(
            config.Mode == ConfigMode.Stack
                ? Stack2LoliTranspiler.Transpile(config.Stack)
                : config.LoliCodeScript,
            config.StartupLoliCodeScript,
            [.. config.Settings.ScriptSettings.CustomUsings]);

        return JsonSerializer.Serialize(result, JsonSerializerOptions);
    }

    /// <summary>
    /// Updates the LoliCode making data of a config.
    /// </summary>
    [McpServerTool(Name = "update_config_lolicode"),
     Description("Updates a config's main LoliCode script, startup script, and custom usings, and returns either a minimal JSON acknowledgment or a fixable validation error.")]
    public async Task<string> UpdateConfigLoliCode(
        string configId,
        ConfigLoliCodeUpdateInput loliCode,
        ConfigService configService,
        IObjectMapper mapper,
        IValidator<ConfigScriptSettingsDto> validator,
        CancellationToken cancellationToken)
    {
        var scriptSettings = new ConfigScriptSettingsDto
        {
            CustomUsings = loliCode.CustomUsings
        };

        var validationResult = await validator.ValidateAsync(scriptSettings, cancellationToken);

        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(Environment.NewLine,
                validationResult.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct());

            return JsonSerializer.Serialize(new UpdateResult(false, errorMessage), JsonSerializerOptions);
        }

        var config = GetConfig(configService, configId);

        if (config.IsRemote)
        {
            throw new ActionNotAllowedException(
                ErrorCode.ActionNotAllowedForRemoteConfig,
                $"Attempted to edit a remote config with id {configId}");
        }

        try
        {
            var stack = Loli2StackTranspiler.Transpile(loliCode.Script);

            var tempSettingsDto = mapper.Map<ConfigSettingsDto>(config.Settings);
            tempSettingsDto.ScriptSettings.CustomUsings = [.. loliCode.CustomUsings];
            var tempSettings = mapper.Map<RuriLib.Models.Configs.ConfigSettings>(tempSettingsDto);

            _ = Loli2CSharpTranspiler.Transpile(loliCode.StartupScript, tempSettings);

            config.Mode = ConfigMode.LoliCode;
            config.LoliCodeScript = loliCode.Script;
            config.StartupLoliCodeScript = loliCode.StartupScript;
            config.Settings.ScriptSettings.CustomUsings = [.. loliCode.CustomUsings];
            config.Stack = stack;

            await configService.SaveAsync(config);
        }
        catch (LoliCodeParsingException ex)
        {
            return JsonSerializer.Serialize(new UpdateResult(false, ex.Message), JsonSerializerOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new UpdateResult(false, ex.Message), JsonSerializerOptions);
        }

        return JsonSerializer.Serialize(new UpdateResult(true), JsonSerializerOptions);
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

    private sealed record ConfigLoliCodeResult(
        string Script,
        string StartupScript,
        List<string> CustomUsings);

    private sealed record ConvertLoliCodeResult(
        bool Converted,
        string CSharpScript,
        string StartupCSharpScript,
        string? Error = null);

    private sealed record UpdateResult(bool Updated, string? Error = null);

    /// <summary>
    /// MCP input for updating a config's LoliCode making data.
    /// </summary>
    public sealed class ConfigLoliCodeUpdateInput
    {
        /// <summary>
        /// The main LoliCode script.
        /// </summary>
        public string Script { get; init; } = string.Empty;

        /// <summary>
        /// The startup LoliCode script.
        /// </summary>
        public string StartupScript { get; init; } = string.Empty;

        /// <summary>
        /// The custom usings used when compiling the generated C# script.
        /// </summary>
        public List<string> CustomUsings { get; init; } = [];
    }
}
