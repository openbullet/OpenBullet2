using OpenBullet2.Entities;

namespace OpenBullet2.Repositories
{
    public class DbTriggeredActionRepository : DbRepository<TriggeredActionEntity>, ITriggeredActionRepository
    {
        public DbTriggeredActionRepository(ApplicationDbContext context)
            : base(context)
        {

        }
    }
}
