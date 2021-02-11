using RuriLib.Providers.Proxies;
using System;

namespace RuriLib.Tests.Utils.Mockup
{
    public class MockedProxySettingsProvider : IProxySettingsProvider
    {
        public TimeSpan ConnectTimeout => TimeSpan.FromSeconds(10);

        public TimeSpan ReadWriteTimeout => TimeSpan.FromSeconds(10);

        public bool ContainsBanKey(string text, bool caseSensitive = false) => false;
        public bool ContainsRetryKey(string text, bool caseSensitive = false) => false;
    }
}
