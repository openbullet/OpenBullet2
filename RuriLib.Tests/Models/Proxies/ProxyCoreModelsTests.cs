using RuriLib.Models.Proxies;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Proxies;

public class ProxyCoreModelsTests
{
    [Fact]
    public void ProxyPoolOptions_DefaultsToCommonProxyTypes()
    {
        var options = new ProxyPoolOptions();

        Assert.Equal([ProxyType.Http, ProxyType.Socks4, ProxyType.Socks5], options.AllowedTypes);
    }

    [Fact]
    public void ProxySource_Defaults_AreSafe()
    {
        var source = new TestProxySource();

        Assert.Equal(ProxyType.Http, source.DefaultType);
        Assert.Equal(string.Empty, source.DefaultUsername);
        Assert.Equal(string.Empty, source.DefaultPassword);
        Assert.Equal(0, source.UserId);
    }

    private sealed class TestProxySource : ProxySource
    {
        public override Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<Proxy>>([]);
    }
}
