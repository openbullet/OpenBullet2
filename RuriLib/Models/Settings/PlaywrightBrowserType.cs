namespace RuriLib.Models.Settings;

/// <summary>
/// Identifies the Playwright browser family to launch.
/// </summary>
public enum PlaywrightBrowserType
{
    /// <summary>
    /// Playwright-managed Chromium.
    /// </summary>
    Chromium,

    /// <summary>
    /// Playwright-managed Firefox.
    /// </summary>
    Firefox,

    /// <summary>
    /// Playwright-managed WebKit.
    /// </summary>
    Webkit
}
