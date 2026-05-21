namespace RuriLib.Models.Settings;

/// <summary>
/// Selects how Playwright should locate the browser executable.
/// </summary>
public enum PlaywrightBrowserSource
{
    /// <summary>
    /// Use the browser managed by Playwright.
    /// </summary>
    Managed,

    /// <summary>
    /// Use a browser executable from an explicit path.
    /// </summary>
    ExecutablePath
}
