using RuriLib.Models.Settings;
using System;

namespace RuriLib.Providers.Proxies
{
    public interface IProxySettingsProvider
    {
        TimeSpan ConnectTimeout { get; }
        TimeSpan ReadWriteTimeout { get; }
        bool ContainsBanKey(string text, bool caseSensitive = false);
        bool ContainsRetryKey(string text, bool caseSensitive = false);
    }
}
