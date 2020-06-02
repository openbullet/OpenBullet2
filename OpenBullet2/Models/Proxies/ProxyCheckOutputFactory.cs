using OpenBullet2.Repositories;
using RuriLib.Models.Proxies;
using System;

namespace OpenBullet2.Models.Proxies
{
    public class ProxyCheckOutputFactory
    {
        private readonly IProxyRepository proxyRepo;

        public ProxyCheckOutputFactory(IProxyRepository proxyRepo)
        {
            this.proxyRepo = proxyRepo;
        }

        public IProxyCheckOutput FromOptions(ProxyCheckOutputOptions options)
        {
            IProxyCheckOutput output = options switch
            {
                DatabaseProxyCheckOutputOptions _ => new DatabaseProxyCheckOutput(proxyRepo),
                _ => throw new NotImplementedException()
            };

            return output;
        }
    }
}
