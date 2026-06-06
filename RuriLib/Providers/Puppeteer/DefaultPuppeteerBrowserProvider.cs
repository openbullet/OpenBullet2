using RuriLib.Services;

namespace RuriLib.Providers.Puppeteer;

/// <summary>
/// Default implementation of <see cref="IPuppeteerBrowserProvider"/>.
/// </summary>
public class DefaultPuppeteerBrowserProvider : IPuppeteerBrowserProvider
{
    /// <summary>
    /// Gets the Chrome binary location.
    /// </summary>
    public string ChromeBinaryLocation { get; }

    /// <summary>
    /// Creates a provider from the persisted RuriLib settings.
    /// </summary>
    /// <param name="settings">The settings service to read from.</param>
    public DefaultPuppeteerBrowserProvider(RuriLibSettingsService settings)
    {
        ChromeBinaryLocation = settings.RuriLibSettings.PuppeteerSettings.ChromeBinaryLocation;
    }
}
