using RuriLib.Providers.Captchas;
using RuriLib.Providers.Emails;
using RuriLib.Providers.Proxies;
using RuriLib.Providers.Puppeteer;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.Security;
using RuriLib.Providers.Selenium;
using RuriLib.Providers.UserAgents;
using RuriLib.Services;

namespace RuriLib.Models.Bots
{
    /// <summary>
    /// The whole purpose of this class is to hide RuriLib settings from the config.
    /// It's probably overengineered right now but at least it's future-proof.
    /// </summary>
    public class Providers
    {
        public IRandomUAProvider RandomUA { get; set; }
        public ICaptchaProvider Captcha { get; set; }
        public IEmailDomainRepository EmailDomains { get; set; }
        public IRNGProvider RNG { get; set; }
        public IPuppeteerBrowserProvider PuppeteerBrowser { get; set; }
        public ISeleniumBrowserProvider SeleniumBrowser { get; set; }
        public IGeneralSettingsProvider GeneralSettings { get; set; }
        public IProxySettingsProvider ProxySettings { get; set; }
        public ISecurityProvider Security { get; set; }

        /// <summary>
        /// Initializes all default providers.
        /// </summary>
        public Providers(RuriLibSettingsService settings)
        {
            if (settings != null)
            {
                RandomUA = new DefaultRandomUAProvider(settings);
                EmailDomains = new FileEmailDomainRepository();
                Captcha = new CaptchaSharpProvider(settings);
                PuppeteerBrowser = new DefaultPuppeteerBrowserProvider(settings);
                SeleniumBrowser = new DefaultSeleniumBrowserProvider(settings);
                GeneralSettings = new DefaultGeneralSettingsProvider(settings);
                ProxySettings = new DefaultProxySettingsProvider(settings);
                Security = new DefaultSecurityProvider(settings);
            }

            RNG = new DefaultRNGProvider();
        }
    }
}
