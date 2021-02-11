using RuriLib.Models.Proxies;
using Xunit;

namespace RuriLib.Tests.Models.Proxies
{
    public class ProxyTests
    {
        [Fact]
        public void Parse_HostAndPort_ParseCorrectly()
        {
            var proxy = Proxy.Parse("127.0.0.1:8000");
            Assert.Equal("127.0.0.1", proxy.Host);
            Assert.Equal(8000, proxy.Port);
        }

        [Fact]
        public void Parse_TypeHostAndPort_ParseType()
        {
            var proxy = Proxy.Parse("(socks5)127.0.0.1:8000");
            Assert.Equal("127.0.0.1", proxy.Host);
            Assert.Equal(8000, proxy.Port);
            Assert.Equal(ProxyType.Socks5, proxy.Type);
        }

        [Fact]
        public void Parse_HostPortUserPass_ParseCredentials()
        {
            var proxy = Proxy.Parse("127.0.0.1:8000:user:pass");
            Assert.Equal("127.0.0.1", proxy.Host);
            Assert.Equal(8000, proxy.Port);
            Assert.Equal("user", proxy.Username);
            Assert.Equal("pass", proxy.Password);
        }
    }
}
