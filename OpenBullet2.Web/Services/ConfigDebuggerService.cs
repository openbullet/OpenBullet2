using RuriLib.Models.Configs;
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

    /// <summary></summary>
    public ConfigDebuggerService(PluginRepository pluginRepo,
        IRandomUAProvider randomUAProvider, IRNGProvider rngProvider,
        RuriLibSettingsService rlSettingsService)
    {
        _pluginRepo = pluginRepo;
        _randomUAProvider = randomUAProvider;
        _rngProvider = rngProvider;
        _rlSettingsService = rlSettingsService;
    }

    /// <summary>
    /// Gets an existing <see cref="ConfigDebugger"/> for the given
    /// config or creates a new one.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public ConfigDebugger Get(Config config, DebuggerOptions options)
    {
        // If we already have a debugger for this config,
        // return the existing debugger instance.
        if (_debuggers.ContainsKey(config.Id))
        {
            return _debuggers[config.Id];
        }

        // Otherwise, start a brand new one.
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
