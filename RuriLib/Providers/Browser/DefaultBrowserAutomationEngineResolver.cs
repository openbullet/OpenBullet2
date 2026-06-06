using RuriLib.Models.Bots;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Settings;
using RuriLib.Providers.Playwright;
using RuriLib.Providers.Puppeteer;
using System;

namespace RuriLib.Providers.Browser;

/// <summary>
/// Default browser automation engine resolver backed by built-in engines.
/// </summary>
public class DefaultBrowserAutomationEngineResolver : IBrowserAutomationEngineResolver
{
    private readonly IBrowserAutomationEngine _puppeteer;
    private readonly IBrowserAutomationEngine _playwright;

    /// <summary>
    /// Creates the resolver with the built-in engine implementations.
    /// </summary>
    public DefaultBrowserAutomationEngineResolver(IPuppeteerBrowserProvider? puppeteerBrowserProvider,
        IPlaywrightBrowserProvider? playwrightBrowserProvider)
    {
        _puppeteer = new PuppeteerBrowserAutomationEngine(puppeteerBrowserProvider ?? new NullPuppeteerBrowserProvider());
        _playwright = new PlaywrightBrowserAutomationEngine(playwrightBrowserProvider ?? new NullPlaywrightBrowserProvider());
    }

    /// <inheritdoc />
    public IBrowserAutomationEngine Resolve(BrowserAutomationEngine engine)
        => engine switch
        {
            BrowserAutomationEngine.Puppeteer => _puppeteer,
            BrowserAutomationEngine.Playwright => _playwright,
            _ => throw new NotSupportedException($"Browser automation engine {engine} is not supported")
        };

    /// <inheritdoc />
    public IBrowserAutomationEngine Resolve(BotData data)
        => Resolve(data.ConfigSettings.BrowserSettings.Engine);

    private class NullPuppeteerBrowserProvider : IPuppeteerBrowserProvider
    {
        public string ChromeBinaryLocation => string.Empty;
    }

    private class NullPlaywrightBrowserProvider : IPlaywrightBrowserProvider
    {
        public PlaywrightBrowserType BrowserType => PlaywrightBrowserType.Chromium;
        public PlaywrightBrowserSource Source => PlaywrightBrowserSource.Managed;
        public string ExecutablePath => string.Empty;
    }
}
