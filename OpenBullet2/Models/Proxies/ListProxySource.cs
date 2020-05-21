using RuriLib.Extensions;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Proxies
{
    public class ListProxySource : IProxySource
    {
        private readonly List<Proxy> proxies;
        private readonly Random random = new Random();

        public ListProxySource(List<Proxy> proxies)
        {
            this.proxies = proxies;
        }

        public Task<ProxyPool> GetAll(bool shuffle = false)
        {
            if (shuffle)
                proxies.Shuffle(random);

            return Task.FromResult(new ProxyPool(proxies));
        }
    }
}
