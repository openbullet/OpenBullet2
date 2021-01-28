using RuriLib.Models.Settings;

namespace RuriLib.Providers.Proxies
{
    public interface IProxySettingsProvider
    {
        ProxySettings Settings { get; }
        bool ContainsBanKey(string text, bool caseSensitive = false);
        bool ContainsRetryKey(string text, bool caseSensitive = false);
    }
}
