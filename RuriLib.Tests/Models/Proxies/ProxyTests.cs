using RuriLib.Models.Proxies;
using Xunit;

namespace RuriLib.Tests.Models.Proxies
{
    public class ProxyTests
    {
        private readonly string simpleProxy = "127.0.0.1:8000";
        private readonly string proxyWithType = "(socks5)127.0.0.1:8000";
        private readonly string proxyWithAuthentication = "127.0.0.1:8000:user:pass";

        [Fact]
        public void Parse_HostAndPort_ParseCorrectly()
        {
            Proxy proxy = Proxy.Parse(simpleProxy);
            Assert.Equal("127.0.0.1", proxy.Host);
            Assert.Equal(8000, proxy.Port);
        }

        [Fact]
        public void Parse_TypeHostAndPort_ParseType()
        {
            Proxy proxy = Proxy.Parse(proxyWithType);
            Assert.Equal("127.0.0.1", proxy.Host);
            Assert.Equal(8000, proxy.Port);
            Assert.Equal(ProxyType.Socks5, proxy.Type);
        }

        [Fact]
        public void Parse_HostPortUserPass_ParseCredentials()
        {
            Proxy proxy = Proxy.Parse(proxyWithAuthentication);
            Assert.Equal("127.0.0.1", proxy.Host);
            Assert.Equal(8000, proxy.Port);
            Assert.Equal("user", proxy.Username);
            Assert.Equal("pass", proxy.Password);
        }
    }
}
