using OpenBullet2.Services;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Models.Proxies.Sources
{
    public class GroupProxySource : ProxySource, IDisposable
    {
        private readonly ProxyReloadService reloadService;

        public int GroupId { get; set; }

        public GroupProxySource(int groupId, ProxyReloadService reloadService)
        {
            GroupId = groupId;
            this.reloadService = reloadService;
        }

        public async override Task<IEnumerable<Proxy>> GetAll()
            => await reloadService.Reload(GroupId, UserId);

        public void Dispose() => reloadService?.Dispose();
    }
}
