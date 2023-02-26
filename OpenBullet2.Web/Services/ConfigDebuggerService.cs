using OpenBullet2.Core.Services;
using RuriLib.Models.Debugger;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;

namespace OpenBullet2.Web.Services;

/// <summary>
/// Holds references to <see cref="ConfigDebugger"/> instances.
/// This holds 1 instance for each config.
/// </summary>
public class ConfigDebuggerService : IDisposable
{
    private readonly Dictionary<string, ConfigDebugger> _debuggers = new();
    private readonly PluginRepository _pluginRepo;
    private readonly IRandomUAProvider _randomUAProvider;
    private readonly IRNGProvider _rngProvider;
    private readonly RuriLibSettingsService _rlSettingsService;
    private readonly ConfigService _configService;

    /// <summary></summary>
    public ConfigDebuggerService(PluginRepository pluginRepo,
        IRandomUAProvider randomUAProvider, IRNGProvider rngProvider,
        RuriLibSettingsService rlSettingsService, ConfigService configService)
    {
        _pluginRepo = pluginRepo;
        _randomUAProvider = randomUAProvider;
        _rngProvider = rngProvider;
        _rlSettingsService = rlSettingsService;
        _configService = configService;
    }

    /// <summary>
    /// Gets an existing <see cref="ConfigDebugger"/> for the given
    /// config or returns null if none was created.
    /// </summary>
    public ConfigDebugger? TryGet(string configId)
        => _debuggers.TryGetValue(configId, out var value) ? value : null;

    /// <summary>
    /// Starts the debugger for the given config with the given options.
    /// </summary>
    public ConfigDebugger CreateNew(string configId, DebuggerOptions options)
    {
        var config = _configService.Configs.FirstOrDefault(c => c.Id == configId);

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
                throw new Exception("The debugger status is not idle, so it cannot be started");
            }

            _debuggers.Remove(config.Id);

            // This will also remove all the event listeners
            existing.Dispose();
        }

        // Create the new instance
        var debugger = new ConfigDebugger(config, options)
        {
            PluginRepo = _pluginRepo,
            RandomUAProvider = _randomUAProvider,
            RNGProvider = _rngProvider,
            RuriLibSettings = _rlSettingsService
        };

        _debuggers[config.Id] = debugger;

        return debugger;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var debugger in _debuggers.Values)
        {
            debugger.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}
