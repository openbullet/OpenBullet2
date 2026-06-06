using RuriLib.Models.Settings;

namespace RuriLib.Providers.Playwright;

/// <summary>
/// Provides Playwright browser configuration.
/// </summary>
public interface IPlaywrightBrowserProvider
{
    /// <summary>
    /// Gets the configured browser family.
    /// </summary>
    PlaywrightBrowserType BrowserType { get; }

    /// <summary>
    /// Gets how the browser executable should be resolved.
    /// </summary>
    PlaywrightBrowserSource Source { get; }

    /// <summary>
    /// Gets the explicit executable path used in <see cref="PlaywrightBrowserSource.ExecutablePath"/> mode.
    /// </summary>
    string ExecutablePath { get; }

}
