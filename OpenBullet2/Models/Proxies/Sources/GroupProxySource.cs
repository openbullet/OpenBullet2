using OpenBullet2.Services;
using RuriLib.Models.Proxies;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Proxies.Sources
{
    public class GroupProxySource : ProxySource
    {
        private readonly ProxyReloadService reloadService;

        public int GroupId { get; set; }

        public GroupProxySource(int groupId, ProxyReloadService reloadService)
        {
            GroupId = groupId;
            this.reloadService = reloadService;
        }

        public override async Task<IEnumerable<Proxy>> GetAll()
            => await reloadService.Reload(GroupId);
    }
}
