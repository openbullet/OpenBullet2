using OpenBullet2.Models.Proxies.Sources;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Proxies
{
    public class ProxySourceFactoryService
    {
        private readonly ProxyReloadService reloadService;

        public ProxySourceFactoryService(ProxyReloadService reloadService)
        {
            this.reloadService = reloadService;
        }

        public Task<ProxySource> FromOptions(ProxySourceOptions options)
        {
            ProxySource source = options switch
            {
                RemoteProxySourceOptions x => new RemoteProxySource(x.Url) { DefaultType = x.DefaultType },
                FileProxySourceOptions x => new FileProxySource(x.FileName) { DefaultType = x.DefaultType },
                GroupProxySourceOptions x => new GroupProxySource(x.GroupId, reloadService),
                _ => throw new NotImplementedException()
            };

            return Task.FromResult(source);
        }
    }
}
