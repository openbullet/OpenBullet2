using RuriLib.Models.Bots;
using RuriLib.Models.Configs.Settings;

namespace RuriLib.Providers.Browser;

/// <summary>
/// Resolves the configured browser automation engine for a bot execution.
/// </summary>
public interface IBrowserAutomationEngineResolver
{
    /// <summary>
    /// Resolves an engine by its configured enum value.
    /// </summary>
    IBrowserAutomationEngine Resolve(BrowserAutomationEngine engine);

    /// <summary>
    /// Resolves the engine configured on the current bot data.
    /// </summary>
    IBrowserAutomationEngine Resolve(BotData data);
}
