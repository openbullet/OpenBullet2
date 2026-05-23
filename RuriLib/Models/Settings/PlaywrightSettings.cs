namespace RuriLib.Models.Settings;

/// <summary>
/// Stores Playwright browser settings.
/// </summary>
public class PlaywrightSettings
{
    /// <summary>
    /// Gets or sets the Playwright browser family to launch.
    /// </summary>
    public PlaywrightBrowserType BrowserType { get; set; } = PlaywrightBrowserType.Chromium;

    /// <summary>
    /// Gets or sets how the browser executable should be resolved.
    /// </summary>
    public PlaywrightBrowserSource Source { get; set; } = PlaywrightBrowserSource.Managed;

    /// <summary>
    /// Gets or sets the explicit browser executable path used in <see cref="PlaywrightBrowserSource.ExecutablePath"/> mode.
    /// </summary>
    public string ExecutablePath { get; set; } = string.Empty;

}
