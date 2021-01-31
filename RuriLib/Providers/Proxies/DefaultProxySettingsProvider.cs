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

        public bool ContainsBanKey(string text, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return settings.GlobalBanKeys.Any(k => text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }

        public bool ContainsRetryKey(string text, bool caseSensitive = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            return settings.GlobalRetryKeys.Any(k => text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
        }
    }
}
