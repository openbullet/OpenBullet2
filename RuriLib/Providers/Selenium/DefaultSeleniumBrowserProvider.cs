using RuriLib.Models.Settings;
using RuriLib.Services;

namespace RuriLib.Providers.Selenium;

/// <summary>
/// Default implementation of <see cref="ISeleniumBrowserProvider"/>.
/// </summary>
public class DefaultSeleniumBrowserProvider : ISeleniumBrowserProvider
{
    /// <summary>
    /// Gets the Chrome binary location.
    /// </summary>
    public string ChromeBinaryLocation { get; }

    /// <summary>
    /// Gets the Firefox binary location.
    /// </summary>
    public string FirefoxBinaryLocation { get; }

    /// <summary>
    /// Gets the configured Selenium browser type.
    /// </summary>
    public SeleniumBrowserType BrowserType { get; }

    /// <summary>
    /// Creates a provider from the persisted RuriLib settings.
    /// </summary>
    public DefaultSeleniumBrowserProvider(RuriLibSettingsService settings)
    {
        ChromeBinaryLocation = settings.RuriLibSettings.SeleniumSettings.ChromeBinaryLocation;
        FirefoxBinaryLocation = settings.RuriLibSettings.SeleniumSettings.FirefoxBinaryLocation;
        BrowserType = settings.RuriLibSettings.SeleniumSettings.BrowserType;
    }
}
