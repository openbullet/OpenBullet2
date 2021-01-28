using RuriLib.Providers.Captchas;
using RuriLib.Providers.Proxies;
using RuriLib.Providers.Puppeteer;
using RuriLib.Providers.RandomNumbers;
using RuriLib.Providers.Security;
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
        public IRNGProvider RNG { get; set; }
        public IPuppeteerBrowserProvider PuppeteerBrowser { get; set; }
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
                Captcha = new CaptchaSharpProvider(settings);
                PuppeteerBrowser = new DefaultPuppeteerBrowserProvider(settings);
                ProxySettings = new DefaultProxySettingsProvider(settings);
                Security = new DefaultSecurityProvider(settings);
            }

            RNG = new DefaultRNGProvider();
        }
    }
}
