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
    }
}
