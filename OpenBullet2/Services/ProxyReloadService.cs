using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using OpenBullet2.Models.Proxies;
using OpenBullet2.Repositories;
using RuriLib.Models.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class ProxyReloadService : IDisposable
    {
        private readonly IProxyGroupRepository proxyGroupsRepo;
        private readonly IProxyRepository proxyRepo;
        private readonly SemaphoreSlim semaphore;

        public ProxyReloadService(IProxyGroupRepository proxyGroupsRepo, IProxyRepository proxyRepo)
        {
            this.proxyGroupsRepo = proxyGroupsRepo;
            this.proxyRepo = proxyRepo;
            semaphore = new SemaphoreSlim(1, 1);
        }

        public void Dispose() => semaphore?.Dispose();

        public async Task<IEnumerable<Proxy>> Reload(int groupId)
        {
            List<ProxyEntity> entities;

            // Only allow reloading one group at a time (multiple threads should
            // not use the same DbContext at the same time).
            await semaphore.WaitAsync();

            try
            {
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
            }
            finally
            {
                semaphore.Release();
            }

            var proxyFactory = new ProxyFactory();
            return entities.Select(e => proxyFactory.FromEntity(e));
        }
    }
}
