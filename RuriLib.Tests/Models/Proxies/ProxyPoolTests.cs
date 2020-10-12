using RuriLib.Models.Proxies;
using Xunit;

namespace RuriLib.Tests.Models.Proxies
{
    public class ProxyPoolTests
    {
        [Fact]
        public void RemoveDuplicates_ListWithDuplicates_ReturnDistinct()
        {
            ProxyPool pool = new ProxyPool(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000),
                new Proxy("127.0.0.1", 8000)
            }, false);

            pool.RemoveDuplicates();
            Assert.Single(pool.Proxies);
        }

        [Fact]
        public void GetProxy_Available_ReturnValidProxy()
        {
            ProxyPool pool = new ProxyPool(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000)
            }, false);

            Assert.NotNull(pool.GetProxy());
        }

        [Fact]
        public void GetProxy_AllBusy_ReturnNull()
        {
            ProxyPool pool = new ProxyPool(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            }, false);

            Assert.Null(pool.GetProxy());
        }

        [Fact]
        public void GetProxy_EvenBusy_ReturnValidProxy()
        {
            ProxyPool pool = new ProxyPool(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { ProxyStatus = ProxyStatus.Busy }
            }, false);

            Assert.NotNull(pool.GetProxy(true));
        }

        [Fact]
        public void GetProxy_MaxUses_ReturnNull()
        {
            ProxyPool pool = new ProxyPool(new Proxy[]
            {
                new Proxy("127.0.0.1", 8000) { TotalUses = 3 }
            }, false);

            Assert.Null(pool.GetProxy(true, 3));
        }
    }
}
