using RuriLib.Models.Settings;
using RuriLib.Services;
using System;
using System.Linq;

namespace RuriLib.Providers.Proxies
{
    public class DefaultGlobalProxyKeysProvider : IGlobalProxyKeysProvider
    {
        private readonly ProxySettings settings;

        public DefaultGlobalProxyKeysProvider(RuriLibSettingsService settings)
        {
            this.settings = settings.RuriLibSettings.ProxySettings;
        }

        public bool ContainsBanKey(string text, bool caseSensitive = false)
            => settings.GlobalBanKeys.Any(k => k.Contains(text,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));

        public bool ContainsRetryKey(string text, bool caseSensitive = false)
            => settings.GlobalRetryKeys.Any(k => k.Contains(text,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase));
    }
}
