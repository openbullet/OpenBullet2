using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores proxies to a database.
    /// </summary>
    public class DbProxyRepository : DbRepository<ProxyEntity>, IProxyRepository
    {
        public DbProxyRepository(ApplicationDbContext context)
            : base(context)
        {
            
        }

        public async override Task Update(ProxyEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await base.Update(entity);
        }

        public async override Task Update(IEnumerable<ProxyEntity> entities)
        {
            foreach (var entity in entities)
            {
                context.Entry(entity).State = EntityState.Modified;
            }

            await base.Update(entities);
        }

        /// <inheritdoc/>
        public async Task RemoveDuplicates(int groupId)
        {
            var proxies = await GetAll()
                .Where(p => p.Group.Id == groupId)
                .ToListAsync();

            var duplicates = proxies
                .GroupBy(p => new { p.Type, p.Host, p.Port, p.Username, p.Password })
                .SelectMany(g => g.Skip(1));
            
            await Delete(duplicates);
        }
    }
}
