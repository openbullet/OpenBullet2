using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.ConfigDebugger;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.SignalR;
using OpenBullet2.Web.Utils;
using RuriLib.Logging;
using RuriLib.Models.Debugger;
using RuriLib.Models.Variables;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Holds references to <see cref="ConfigDebugger" /> instances.
/// This holds 1 instance for each config.
/// </summary>
public sealed class ConfigDebuggerService : IDisposable
{
    private readonly ConfigService _configService;

    // Maps debuggers to connections
    private readonly Dictionary<ConfigDebugger, List<string>> _connections = new();

    // Maps config IDs to debuggers
    private readonly Dictionary<string, ConfigDebugger> _debuggers = new();
    private readonly IHubContext<ConfigDebuggerHub> _hub;
    private readonly ILogger<ConfigDebuggerService> _logger;

    // Event handlers
    private readonly EventHandler<BotLoggerEntry> _onNewLog;
    private readonly EventHandler<ConfigDebuggerStatus> _onStatusChanged;
    private readonly PluginRepository _pluginRepo;
    private readonly IRandomUAProvider _randomUAProvider;
    private readonly RuriLibSettingsService _rlSettingsService;
    private readonly IRNGProvider _rngProvider;

    /// <summary></summary>
    public ConfigDebuggerService(PluginRepository pluginRepo,
        IRandomUAProvider randomUAProvider, IRNGProvider rngProvider,
        RuriLibSettingsService rlSettingsService, ConfigService configService,
        IHubContext<ConfigDebuggerHub> hub, ILogger<ConfigDebuggerService> logger)
    {
        _pluginRepo = pluginRepo;
        _randomUAProvider = randomUAProvider;
        _rngProvider = rngProvider;
        _rlSettingsService = rlSettingsService;
        _configService = configService;
        _hub = hub;
        _logger = logger;

        // Create the event handlers
        _onNewLog = EventHandlers.TryAsync<BotLoggerEntry>(
            OnNewLogEntryAsync,
            SendError
        );

        _onStatusChanged = EventHandlers.TryAsync<ConfigDebuggerStatus>(
            OnStatusChangedAsync,
            SendError
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Registers a new connection, a.k.a. a debugging session started
    /// by a given client for a given config.
    /// </summary>
    public void RegisterConnection(string connectionId, string configId)
    {
        // If we don't already have a debugger for this config, create one
        if (!_debuggers.TryGetValue(configId, out var debugger))
        {
            var config = _configService.Configs.Find(c => c.Id == configId);
            debugger = new ConfigDebugger(config);
            _debuggers[configId] = debugger;
            _connections[debugger] = [];

            // Hook the event handlers to the newly created debugger
            debugger.NewLogEntry += _onNewLog;
            debugger.StatusChanged += _onStatusChanged;
        }

        // Add the connection to the list
        _connections[debugger].Add(connectionId);

        _logger.LogDebug("Registered new connection {ConnectionId} for debugger of config {ConfigId}",
            connectionId, configId);
    }

    /// <summary>
    /// Unregisters an existing connection.
    /// </summary>
    public void UnregisterConnection(string connectionId, string configId)
    {
        if (_debuggers.TryGetValue(configId, out var debugger))
        {
            _connections[debugger].Remove(connectionId);
        }

        _logger.LogDebug("Unregistered connection {ConnectionId} for debugger of config {ConfigId}",
            connectionId, configId);
    }

    private async Task OnNewLogEntryAsync(object? sender, BotLoggerEntry e)
    {
        var message = new DbgNewLogMessage { NewMessage = e };

        var debugger = sender as ConfigDebugger;

        await _hub.Clients.Clients(_connections[debugger!]).SendAsync(
            ConfigDebuggerMethods.NewLogEntry, message);
    }

    private async Task OnStatusChangedAsync(object? sender, ConfigDebuggerStatus e)
    {
        var message = new DbgStatusChangedMessage { NewStatus = e };

        var debugger = sender as ConfigDebugger;

        await _hub.Clients.Clients(_connections[debugger!]).SendAsync(
            ConfigDebuggerMethods.StatusChanged, message);

        // Right now, only when the status goes back to idle, we
        // update the variables.
        // TODO: In the future it would be nice to update them more often.
        var varMessage = new DbgVariablesChangedMessage {
            Variables = (sender as ConfigDebugger)!.Options.Variables.Select(MapVariable)
        };

        await _hub.Clients.Clients(_connections[debugger!]).SendAsync(
            ConfigDebuggerMethods.VariablesChanged, varMessage);
    }

    private Task SendError(Exception ex)
    {
        _logger.LogError(ex, "Error while sending message to the client");
        return Task.CompletedTask;
    }

    /// <summary>
    /// Maps a <see cref="Variable" /> to a <see cref="VariableDto" />.
    /// </summary>
    public static VariableDto MapVariable(Variable v)
        => new() {
            Name = v.Name,
            MarkedForCapture = v.MarkedForCapture,
            Type = v.Type,
            Value = v switch {
                StringVariable x => x.AsString(),
                IntVariable x => x.AsInt(),
                FloatVariable x => x.AsFloat(),
                ListOfStringsVariable x => x.AsListOfStrings(),
                DictionaryOfStringsVariable x => x.AsDictionaryOfStrings(),
                BoolVariable x => x.AsBool(),
                ByteArrayVariable x => x.AsByteArray(),
                _ => throw new NotImplementedException()
            }
        };

    /// <summary>
    /// Gets an existing <see cref="ConfigDebugger" /> for the given
    /// config or returns null if none was created.
    /// </summary>
    public ConfigDebugger? TryGet(string configId)
        => _debuggers.TryGetValue(configId, out var value) ? value : null;

    /// <summary>
    /// Starts a debugger for the given config with the given options,
    /// after disposing the previous one (if any).
    /// </summary>
    public void StartNew(string configId, DebuggerOptions options)
    {
        var debugger = CreateNew(configId, options);

        // Fire and forget
        _ = Task.Run(async () =>
        {
            // Wrap everything inside a try/catch so we don't
            // lose the exceptions.
            try
            {
                await debugger.Run();
            }
            catch (Exception ex)
            {
                var message = new ErrorMessage {
                    Type = ex.GetType().Name, Message = ex.Message, StackTrace = ex.ToString()
                };

                await _hub.Clients.Clients(_connections[debugger])
                    .SendAsync(CommonMethods.Error, message);
            }
        });
    }
    
    /// <summary>
    /// Creates a new debugger for the given config with the given options.
    /// </summary>
    public ConfigDebugger Create(
        string configId, DebuggerOptions options)
    {
        // Get the config
        var config = _configService.Configs.Find(c => c.Id == configId);
        
        if (config is null)
        {
            throw new ArgumentException($"Invalid config id: {configId}");
        }
        
        // Create the new instance
        var debugger = new ConfigDebugger(config, options) {
            PluginRepo = _pluginRepo,
            RandomUAProvider = _randomUAProvider,
            RNGProvider = _rngProvider,
            RuriLibSettings = _rlSettingsService
        };
        
        return debugger;
    }

    private ConfigDebugger CreateNew(string configId, DebuggerOptions options)
    {
        // Get the config
        var config = _configService.Configs.Find(c => c.Id == configId);

        if (config is null)
        {
            throw new ArgumentException($"Invalid config id: {configId}");
        }

        // If we already have a debugger, we need to dispose it
        if (_debuggers.TryGetValue(config.Id, out var existing))
        {
            // If it's still running we cannot do that
            if (existing.Status is not ConfigDebuggerStatus.Idle)
            {
                throw new ApiException(ErrorCode.ConfigDebuggerNotIdle,
                    "The debugger status is not idle, so it cannot be started");
            }

            _debuggers.Remove(config.Id);

            // This will also remove all the event listeners
            existing.Dispose();
        }

        // Create the new instance
        var debugger = new ConfigDebugger(config, options) {
            PluginRepo = _pluginRepo,
            RandomUAProvider = _randomUAProvider,
            RNGProvider = _rngProvider,
            RuriLibSettings = _rlSettingsService
        };

        _debuggers[config.Id] = debugger;

        // Transfer the connections from the old debugger to the new one
        if (existing is not null && _connections.TryGetValue(existing, out var connections))
        {
            _connections[debugger] = connections;
            _connections.Remove(existing);
        }
        else
        {
            _connections[debugger] = new List<string>();
        }

        // Hook the events to the newly created debugger
        debugger.NewLogEntry += _onNewLog;
        debugger.StatusChanged += _onStatusChanged;

        return debugger;
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var debugger in _debuggers.Values)
            {
                debugger.Dispose();
            }
        }
    }

    /// <inheritdoc />
    ~ConfigDebuggerService()
    {
        Dispose(false);
    }
}
