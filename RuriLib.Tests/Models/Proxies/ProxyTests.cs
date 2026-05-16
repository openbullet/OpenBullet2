using RuriLib.Models.Proxies;
using Xunit;

namespace RuriLib.Tests.Models.Proxies;

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

    [Fact]
    public void Parse_TypeHostPortUserPass_ParseLegacySyntax()
    {
        var proxy = Proxy.Parse("(socks5)127.0.0.1:8000:user:pass");

        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(8000, proxy.Port);
        Assert.Equal(ProxyType.Socks5, proxy.Type);
        Assert.Equal("user", proxy.Username);
        Assert.Equal("pass", proxy.Password);
    }

    [Fact]
    public void Parse_UriSyntaxWithCredentials_ParseCorrectly()
    {
        var proxy = Proxy.Parse("http://user:pass@127.0.0.1:8000");

        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(8000, proxy.Port);
        Assert.Equal(ProxyType.Http, proxy.Type);
        Assert.Equal("user", proxy.Username);
        Assert.Equal("pass", proxy.Password);
    }

    [Fact]
    public void Parse_HttpsUriSyntax_MapsToHttpProxyType()
    {
        var proxy = Proxy.Parse("https://127.0.0.1:8443");

        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(8443, proxy.Port);
        Assert.Equal(ProxyType.Http, proxy.Type);
    }

    [Theory]
    [InlineData("socks4://127.0.0.1:1080", ProxyType.Socks4)]
    [InlineData("socks4a://127.0.0.1:1080", ProxyType.Socks4a)]
    [InlineData("socks5://127.0.0.1:1080", ProxyType.Socks5)]
    public void Parse_UriSyntax_ParsesProxyScheme(string proxyString, ProxyType expectedType)
    {
        var proxy = Proxy.Parse(proxyString);

        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(1080, proxy.Port);
        Assert.Equal(expectedType, proxy.Type);
    }

    [Fact]
    public void Parse_UriSyntaxWithoutCredentials_UsesDefaultCredentials()
    {
        var proxy = Proxy.Parse("socks5://127.0.0.1:8000", defaultUsername: "user", defaultPassword: "pass");

        Assert.Equal("127.0.0.1", proxy.Host);
        Assert.Equal(8000, proxy.Port);
        Assert.Equal(ProxyType.Socks5, proxy.Type);
        Assert.Equal("user", proxy.Username);
        Assert.Equal("pass", proxy.Password);
    }

    [Fact]
    public void Constructor_WithoutCredentials_LeavesAuthenticationDisabled()
    {
        var proxy = new Proxy("127.0.0.1", 8000);

        Assert.False(proxy.NeedsAuthentication);
        Assert.Null(proxy.Username);
        Assert.Null(proxy.Password);
    }

    [Fact]
    public void GetHashCode_WithNullCredentials_DoesNotThrow()
    {
        var proxy = new Proxy("127.0.0.1", 8000, username: null, password: null);

        var exception = Record.Exception(() => proxy.GetHashCode());

        Assert.Null(exception);
    }

    [Fact]
    public void Protocol_UsesLowercaseTypeName()
    {
        var proxy = new Proxy("127.0.0.1", 8000, ProxyType.Socks5);

        Assert.Equal("socks5", proxy.Protocol);
    }
}
