using Microsoft.EntityFrameworkCore;
using OpenBullet2.Repositories;
using RuriLib.Extensions;
using RuriLib.Models.Proxies;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Models.Proxies.Sources
{
    public class GroupProxySource : ProxySource
    {
        private readonly IProxyGroupRepository proxyGroupsRepo;
        private readonly IProxyRepository proxyRepo;

        public int GroupId { get; set; }

        public GroupProxySource(int groupId, IProxyGroupRepository proxyGroupsRepo, IProxyRepository proxyRepo)
        {
            GroupId = groupId;
            this.proxyGroupsRepo = proxyGroupsRepo;
            this.proxyRepo = proxyRepo;
        }

        public override async Task<IEnumerable<Proxy>> GetAll()
        {
            var group = await proxyGroupsRepo.Get(GroupId);
            var entities = await proxyRepo.GetAll()
                .Where(p => p.GroupId == GroupId)
                .ToListAsync();

            var proxyFactory = new ProxyFactory();
            return entities.Select(e => proxyFactory.FromEntity(e));
        }
    }
}
