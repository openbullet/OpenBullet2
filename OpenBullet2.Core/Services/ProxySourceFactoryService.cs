using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Proxies.Sources;
using RuriLib.Models.Proxies;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Services
{
    /// <summary>
    /// Factory that creates a <see cref="ProxySource"/> from a <see cref="ProxySourceOptions"/> object.
    /// </summary>
    public class ProxySourceFactoryService
    {
        private readonly ProxyReloadService reloadService;

        public ProxySourceFactoryService(ProxyReloadService reloadService)
        {
            this.reloadService = reloadService;
        }

        /// <summary>
        /// Creates a <see cref="ProxySource"/> from a <see cref="ProxySourceOptions"/> object.
        /// </summary>
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
