using RuriLib.Models.Settings;
using RuriLib.Services;
using System;
using System.Linq;

namespace RuriLib.Providers.Proxies
{
    public class DefaultProxySettingsProvider : IProxySettingsProvider
    {
        private readonly ProxySettings settings;

        public DefaultProxySettingsProvider(RuriLibSettingsService settings)
        {
            this.settings = settings.RuriLibSettings.ProxySettings;
        }

        public TimeSpan ConnectTimeout => TimeSpan.FromMilliseconds(settings.ProxyConnectTimeoutMilliseconds);

        public TimeSpan ReadWriteTimeout => TimeSpan.FromMilliseconds(settings.ProxyReadWriteTimeoutMilliseconds);

        public bool ContainsBanKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                matchedKey = null;
                return false;
            }

            matchedKey = settings.GlobalBanKeys.Where(k => !string.IsNullOrEmpty(k)).FirstOrDefault(k => text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));

            return matchedKey != null;
        }

        public bool ContainsRetryKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                matchedKey = null;
                return false;
            }

            matchedKey = settings.GlobalRetryKeys.Where(k => !string.IsNullOrEmpty(k)).FirstOrDefault(k => text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));

            return matchedKey != null;
        }
    }
}
