namespace RuriLib.Providers.Puppeteer;

/// <summary>
/// Provides Puppeteer browser configuration.
/// </summary>
public interface IPuppeteerBrowserProvider
{
    /// <summary>
    /// Gets the Chrome binary location.
    /// </summary>
    string ChromeBinaryLocation { get; }
}
