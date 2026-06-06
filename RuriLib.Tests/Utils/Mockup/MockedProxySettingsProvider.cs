using System;
using RuriLib.Providers.Proxies;

namespace RuriLib.Tests.Utils.Mockup;

public class MockedProxySettingsProvider : IProxySettingsProvider
{
    public TimeSpan ConnectTimeout => TimeSpan.FromSeconds(10);

    public TimeSpan ReadWriteTimeout => TimeSpan.FromSeconds(10);

    public bool ContainsBanKey(string text, out string matchedKey, bool caseSensitive = false)
    {
        matchedKey = string.Empty;
        return false;
    }

    public bool ContainsRetryKey(string text, out string matchedKey, bool caseSensitive = false)
    {
        matchedKey = string.Empty;
        return false;
    }
}
