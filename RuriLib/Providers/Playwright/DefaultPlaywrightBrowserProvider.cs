using RuriLib.Models.Settings;
using RuriLib.Services;

namespace RuriLib.Providers.Playwright;

/// <summary>
/// Default implementation of <see cref="IPlaywrightBrowserProvider"/>.
/// </summary>
public class DefaultPlaywrightBrowserProvider : IPlaywrightBrowserProvider
{
    /// <inheritdoc />
    public PlaywrightBrowserType BrowserType { get; }

    /// <inheritdoc />
    public PlaywrightBrowserSource Source { get; }

    /// <inheritdoc />
    public string ExecutablePath { get; }

    /// <summary>
    /// Creates a provider from the persisted RuriLib settings.
    /// </summary>
    /// <param name="settings">The settings service to read from.</param>
    public DefaultPlaywrightBrowserProvider(RuriLibSettingsService settings)
    {
        BrowserType = settings.RuriLibSettings.PlaywrightSettings.BrowserType;
        Source = settings.RuriLibSettings.PlaywrightSettings.Source;
        ExecutablePath = settings.RuriLibSettings.PlaywrightSettings.ExecutablePath;
    }
}
