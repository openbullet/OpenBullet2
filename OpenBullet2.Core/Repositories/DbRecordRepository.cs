using OpenBullet2.Core.Entities;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores records to a database.
    /// </summary>
    public class DbRecordRepository : DbRepository<RecordEntity>, IRecordRepository
    {
        public DbRecordRepository(ApplicationDbContext context)
            : base(context)
        {

        }
    }
}
