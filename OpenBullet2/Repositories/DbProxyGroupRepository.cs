using OpenBullet2.Entities;

namespace OpenBullet2.Repositories
{
    public class DbProxyGroupRepository : DbRepository<ProxyGroupEntity>, IProxyGroupRepository
    {
        public DbProxyGroupRepository(ApplicationDbContext context)
            : base(context)
        {

        }
    }
}
