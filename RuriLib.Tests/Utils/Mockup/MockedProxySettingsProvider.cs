using RuriLib.Providers.Proxies;
using System;

namespace RuriLib.Tests.Utils.Mockup
{
    public class MockedProxySettingsProvider : IProxySettingsProvider
    {
        public TimeSpan ConnectTimeout => TimeSpan.FromSeconds(10);

        public TimeSpan ReadWriteTimeout => TimeSpan.FromSeconds(10);

        public bool ContainsBanKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            matchedKey = null;
            return false;
        }

        public bool ContainsRetryKey(string text, out string matchedKey, bool caseSensitive = false)
        {
            matchedKey = null;
            return false;
        }
    }
}
