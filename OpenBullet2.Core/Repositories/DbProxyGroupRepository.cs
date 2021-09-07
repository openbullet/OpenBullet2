using OpenBullet2.Core.Entities;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores proxy groups to a database.
    /// </summary>
    public class DbProxyGroupRepository : DbRepository<ProxyGroupEntity>, IProxyGroupRepository
    {
        public DbProxyGroupRepository(ApplicationDbContext context)
            : base(context)
        {

        }
    }
}
