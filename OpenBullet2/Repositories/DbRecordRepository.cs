using OpenBullet2.Entities;

namespace OpenBullet2.Repositories
{
    public class DbRecordRepository : DbRepository<RecordEntity>, IRecordRepository
    {
        public DbRecordRepository(ApplicationDbContext context)
            : base(context)
        {

        }
    }
}
