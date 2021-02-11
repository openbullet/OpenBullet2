using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Models.Proxies
{
    public class ProxyPoolTests
    {
        [Fact]
        public async Task RemoveDuplicates_ListWithDuplicates_ReturnDistinct()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000),
                new Proxy("127.0.0.1", 8000)
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAll();
            pool.RemoveDuplicates();
            Assert.Single(pool.Proxies);
        }

        [Fact]
        public async Task GetProxy_Available_ReturnValidProxy()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000)
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAll();
            Assert.NotNull(pool.GetProxy());
        }

        [Fact]
        public async Task GetProxy_AllBusy_ReturnNull()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAll();
            Assert.Null(pool.GetProxy());
        }

        [Fact]
        public async Task GetProxy_EvenBusy_ReturnValidProxy()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAll();
            Assert.NotNull(pool.GetProxy(true));
        }

        [Fact]
        public async Task GetProxy_MaxUses_ReturnNull()
        {
            ListProxySource source = new(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { TotalUses = 3 }
            });

            var pool = new ProxyPool(new ProxySource[] { source });

            await pool.ReloadAll();
            Assert.Null(pool.GetProxy(true, 3));
        }
    }
}
