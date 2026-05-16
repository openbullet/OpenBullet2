using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.ConfigDebugger;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Services;
using RuriLib.Logging;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;

namespace OpenBullet2.Web.Mcp;

/// <summary>
/// MCP tools related to config debugging.
/// </summary>
[McpServerToolType]
public sealed class ConfigDebugMcpTools
{
    /// <summary>
    /// Debugs a config and streams plain log messages through progress notifications while returning the final result as JSON.
    /// </summary>
    [McpServerTool(Name = "debug_config"),
     Description("Runs the config debugger once for the given config, streams plain log messages through progress notifications, and returns the final log, variables, and error as JSON.")]
    public async Task<string> DebugConfig(
        string configId,
        DebugConfigInput? debug,
        ConfigService configService,
        ConfigDebuggerService configDebuggerService,
        IProgress<ProgressNotificationValue> progress,
        CancellationToken cancellationToken)
    {
        debug ??= new DebugConfigInput();

        _ = configService.Configs.Find(c => c.Id == configId)
            ?? throw new ApiException(ErrorCode.ConfigNotFound,
                $"Config with id {configId} was not found");

        var options = new RuriLib.Models.Debugger.DebuggerOptions
        {
            ProxyType = debug.ProxyType,
            TestData = debug.TestData,
            TestProxy = debug.TestProxy ?? string.Empty,
            UseProxy = !string.IsNullOrWhiteSpace(debug.TestProxy),
            WordlistType = string.IsNullOrWhiteSpace(debug.WordlistType) ? "Default" : debug.WordlistType
        };

        ErrorMessage? error = null;

        using var debugger = configDebuggerService.Create(configId, options);
        var progressCount = 0;

        EventHandler<BotLoggerEntry>? onNewLogEntry = null;
        onNewLogEntry = (_, entry) =>
        {
            if (!string.IsNullOrWhiteSpace(entry.Message))
            {
                progressCount++;
                progress.Report(new ProgressNotificationValue
                {
                    Progress = progressCount,
                    Message = entry.Message
                });
            }
        };

        debugger.NewLogEntry += onNewLogEntry;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await debugger.Run();
        }
        catch (Exception ex)
        {
            error = new ErrorMessage
            {
                Type = ex.GetType().Name,
                Message = ex.Message,
                StackTrace = ex.ToString()
            };
        }
        finally
        {
            debugger.NewLogEntry -= onNewLogEntry;
        }

        var result = new DebugConfigResult(
            error is null,
            debugger.Logger.Entries.Select(e => e.Message).ToList(),
            debugger.Options.Variables.Select(ConfigDebuggerService.MapVariable).ToList(),
            error);

        return JsonSerializer.Serialize(result, JsonSerializerOptions);
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private sealed record DebugConfigResult(
        bool Success,
        List<string> Log,
        List<VariableDto> Variables,
        ErrorMessage? Error);

    /// <summary>
    /// MCP input for debugging a config once.
    /// </summary>
    public sealed class DebugConfigInput
    {
        /// <summary>
        /// The data to test the config with.
        /// </summary>
        public string TestData { get; init; } = string.Empty;

        /// <summary>
        /// The wordlist type to use for the test data.
        /// </summary>
        public string WordlistType { get; init; } = "Default";

        /// <summary>
        /// The proxy to use, if any.
        /// </summary>
        public string? TestProxy { get; init; }

        /// <summary>
        /// The proxy type to use.
        /// </summary>
        public ProxyType ProxyType { get; init; } = ProxyType.Http;
    }
}
