using OpenBullet2.Models.Proxies.Sources;
using OpenBullet2.Repositories;
using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Proxies
{
    public class ProxySourceFactory
    {
        private readonly IProxyGroupRepository proxyGroupsRepo;
        private readonly IProxyRepository proxyRepo;

        public ProxySourceFactory(IProxyGroupRepository proxyGroupsRepo, IProxyRepository proxyRepo)
        {
            this.proxyGroupsRepo = proxyGroupsRepo;
            this.proxyRepo = proxyRepo;
        }

        public Task<ProxySource> FromOptions(ProxySourceOptions options)
        {
            ProxySource source = options switch
            {
                RemoteProxySourceOptions x => new RemoteProxySource(x.Url),
                FileProxySourceOptions x => new FileProxySource(x.FileName),
                GroupProxySourceOptions x => new GroupProxySource(x.GroupId, proxyGroupsRepo, proxyRepo),
                _ => throw new NotImplementedException()
            };

            return Task.FromResult(source);
        }
    }
}
