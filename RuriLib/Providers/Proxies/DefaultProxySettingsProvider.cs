using RuriLib.Models.Settings;
using RuriLib.Services;
using System;
using System.Linq;

namespace RuriLib.Providers.Proxies
{
    public class DefaultProxySettingsProvider : IProxySettingsProvider
    {
        public ProxySettings Settings { get; }

        public DefaultProxySettingsProvider(RuriLibSettingsService settings)
        {
            Settings = settings.RuriLibSettings.ProxySettings;
        }

        public bool ContainsBanKey(string text, bool caseSensitive = false)
            => Settings.GlobalBanKeys.Any(k => text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));

        public bool ContainsRetryKey(string text, bool caseSensitive = false)
            => Settings.GlobalRetryKeys.Any(k => text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
    }
}
