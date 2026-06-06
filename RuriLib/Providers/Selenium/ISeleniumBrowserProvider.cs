using RuriLib.Models.Settings;

namespace RuriLib.Providers.Selenium;

/// <summary>
/// Provides Selenium browser configuration.
/// </summary>
public interface ISeleniumBrowserProvider
{
    /// <summary>
    /// Gets the Chrome binary location.
    /// </summary>
    string ChromeBinaryLocation { get; }

    /// <summary>
    /// Gets the Firefox binary location.
    /// </summary>
    string FirefoxBinaryLocation { get; }

    /// <summary>
    /// Gets the configured Selenium browser type.
    /// </summary>
    SeleniumBrowserType BrowserType { get; }
}
