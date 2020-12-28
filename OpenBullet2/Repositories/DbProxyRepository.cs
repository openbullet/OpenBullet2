using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using OpenBullet2.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class DbProxyRepository : DbRepository<ProxyEntity>, IProxyRepository
    {
        public DbProxyRepository(ApplicationDbContext context)
            : base(context)
        {
            
        }

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
