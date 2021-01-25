using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using Xunit;

namespace RuriLib.Tests.Models.Proxies
{
    public class ProxyPoolTests
    {
        [Fact]
        public void RemoveDuplicates_ListWithDuplicates_ReturnDistinct()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000),
                new Proxy("127.0.0.1", 8000)
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            pool.RemoveDuplicates();
            Assert.Single(pool.Proxies);
        }

        [Fact]
        public void GetProxy_Available_ReturnValidProxy()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000)
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            Assert.NotNull(pool.GetProxy());
        }

        [Fact]
        public void GetProxy_AllBusy_ReturnNull()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            Assert.Null(pool.GetProxy());
        }

        [Fact]
        public void GetProxy_EvenBusy_ReturnValidProxy()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            Assert.NotNull(pool.GetProxy(true));
        }

        [Fact]
        public void GetProxy_MaxUses_ReturnNull()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { TotalUses = 3 }
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            Assert.Null(pool.GetProxy(true, 3));
        }
    }
}
