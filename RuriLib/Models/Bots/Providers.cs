using RuriLib.Providers.Captchas;
using RuriLib.Providers.Browser;
using RuriLib.Providers.Emails;
using RuriLib.Providers.Playwright;
using RuriLib.Providers.Proxies;
using RuriLib.Providers.Puppeteer;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.Security;
using RuriLib.Providers.Selenium;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;

namespace RuriLib.Models.Bots;

/// <summary>
/// The whole purpose of this class is to hide RuriLib settings from the config.
/// It's probably overengineered right now but at least it's future-proof.
/// </summary>
public class Providers
{
    /// <summary>
    /// Gets or sets the random user agent provider.
    /// </summary>
    public IRandomUAProvider RandomUA { get; set; } = null!;

    /// <summary>
    /// Gets or sets the captcha provider.
    /// </summary>
    public ICaptchaProvider Captcha { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email domain repository.
    /// </summary>
    public IEmailDomainRepository EmailDomains { get; set; } = null!;

    /// <summary>
    /// Gets or sets the random number provider.
    /// </summary>
    public IRNGProvider RNG { get; set; }

    /// <summary>
    /// Gets or sets the browser automation engine resolver.
    /// </summary>
    public IBrowserAutomationEngineResolver BrowserAutomation { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Puppeteer browser provider.
    /// </summary>
    public IPuppeteerBrowserProvider PuppeteerBrowser { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Playwright browser provider.
    /// </summary>
    public IPlaywrightBrowserProvider PlaywrightBrowser { get; set; } = null!;

    /// <summary>
    /// Gets or sets the Selenium browser provider.
    /// </summary>
    public ISeleniumBrowserProvider SeleniumBrowser { get; set; } = null!;

    /// <summary>
    /// Gets or sets the general settings provider.
    /// </summary>
    public IGeneralSettingsProvider GeneralSettings { get; set; } = null!;

    /// <summary>
    /// Gets or sets the proxy settings provider.
    /// </summary>
    public IProxySettingsProvider ProxySettings { get; set; } = null!;

    /// <summary>
    /// Gets or sets the security settings provider.
    /// </summary>
    public ISecurityProvider Security { get; set; } = null!;

    /// <summary>
    /// Initializes all default providers.
    /// </summary>
    /// <param name="settings">The settings service used to build the default providers.</param>
    public Providers(RuriLibSettingsService? settings)
    {
        if (settings is not null)
        {
            RandomUA = new DefaultRandomUAProvider(settings);
            EmailDomains = new FileEmailDomainRepository();
            Captcha = new CaptchaSharpProvider(settings);
            PuppeteerBrowser = new DefaultPuppeteerBrowserProvider(settings);
            PlaywrightBrowser = new DefaultPlaywrightBrowserProvider(settings);
            SeleniumBrowser = new DefaultSeleniumBrowserProvider(settings);
            GeneralSettings = new DefaultGeneralSettingsProvider(settings);
            ProxySettings = new DefaultProxySettingsProvider(settings);
            Security = new DefaultSecurityProvider(settings);
        }

        BrowserAutomation = new DefaultBrowserAutomationEngineResolver(PuppeteerBrowser, PlaywrightBrowser);
        RNG = new DefaultRNGProvider();
    }
}
