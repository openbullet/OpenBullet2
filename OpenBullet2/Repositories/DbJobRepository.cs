using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;

namespace OpenBullet2.Repositories
{
    public class DbJobRepository : DbRepository<JobEntity>, IJobRepository
    {
        public DbJobRepository(ApplicationDbContext context)
            : base(context)
        {

        }

        public void Purge()
        {
            context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Jobs)}");
        }
    }
}
