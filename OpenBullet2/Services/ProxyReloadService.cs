using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using OpenBullet2.Models.Proxies;
using OpenBullet2.Repositories;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class ProxyReloadService
    {
        private readonly IProxyGroupRepository proxyGroupsRepo;
        private readonly IProxyRepository proxyRepo;

        public ProxyReloadService(IProxyGroupRepository proxyGroupsRepo, IProxyRepository proxyRepo)
        {
            this.proxyGroupsRepo = proxyGroupsRepo;
            this.proxyRepo = proxyRepo;
        }

        public async Task<IEnumerable<Proxy>> Reload(int groupId)
        {
            List<ProxyEntity> entities;

            if (groupId == -1)
            {
                entities = await proxyRepo.GetAll().ToListAsync();
            }
            else
            {
                var group = await proxyGroupsRepo.Get(groupId);
                entities = await proxyRepo.GetAll()
                    .Where(p => p.Group.Id == groupId)
                    .ToListAsync();
            }

            var proxyFactory = new ProxyFactory();
            return entities.Select(e => proxyFactory.FromEntity(e));
        }
    }
}
