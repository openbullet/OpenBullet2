using RuriLib.Models.Bots;
using RuriLib.Models.Configs.Settings;
using RuriLib.Providers.Puppeteer;
using System;

namespace RuriLib.Providers.Browser;

/// <summary>
/// Default browser automation engine resolver backed by built-in engines.
/// </summary>
public class DefaultBrowserAutomationEngineResolver : IBrowserAutomationEngineResolver
{
    private readonly IBrowserAutomationEngine _puppeteer;

    /// <summary>
    /// Creates the resolver with the built-in engine implementations.
    /// </summary>
    public DefaultBrowserAutomationEngineResolver(IPuppeteerBrowserProvider? puppeteerBrowserProvider)
    {
        _puppeteer = new PuppeteerBrowserAutomationEngine(puppeteerBrowserProvider ?? new NullPuppeteerBrowserProvider());
    }

    /// <inheritdoc />
    public IBrowserAutomationEngine Resolve(BrowserAutomationEngine engine)
        => engine switch
        {
            BrowserAutomationEngine.Puppeteer => _puppeteer,
            _ => throw new NotSupportedException($"Browser automation engine {engine} is not supported")
        };

    /// <inheritdoc />
    public IBrowserAutomationEngine Resolve(BotData data)
        => Resolve(data.ConfigSettings.BrowserSettings.Engine);

    private class NullPuppeteerBrowserProvider : IPuppeteerBrowserProvider
    {
        public string ChromeBinaryLocation => string.Empty;
    }
}
