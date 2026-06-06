namespace RuriLib.Providers.Proxies;

/// <summary>
/// Provides general runtime settings.
/// </summary>
public interface IGeneralSettingsProvider
{
    /// <summary>
    /// Gets a value indicating whether verbose logging is enabled.
    /// </summary>
    bool VerboseMode { get; }

    /// <summary>
    /// Gets a value indicating whether all results should be logged.
    /// </summary>
    bool LogAllResults { get; }
}
