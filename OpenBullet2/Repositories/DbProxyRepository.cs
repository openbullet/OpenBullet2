using OpenBullet2.Entities;

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
