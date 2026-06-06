using RuriLib.Models.Settings;
using RuriLib.Services;

namespace RuriLib.Providers.Proxies;

/// <summary>
/// Default implementation of <see cref="IGeneralSettingsProvider"/>.
/// </summary>
public class DefaultGeneralSettingsProvider : IGeneralSettingsProvider
{
    private readonly GeneralSettings settings;

    /// <summary>
    /// Creates a provider from the persisted RuriLib settings.
    /// </summary>
    public DefaultGeneralSettingsProvider(RuriLibSettingsService settings)
    {
        this.settings = settings.RuriLibSettings.GeneralSettings;
    }

    /// <summary>
    /// Gets a value indicating whether verbose logging is enabled.
    /// </summary>
    public bool VerboseMode => settings.VerboseMode;

    /// <summary>
    /// Gets a value indicating whether all results should be logged.
    /// </summary>
    public bool LogAllResults => settings.LogAllResults;
}
