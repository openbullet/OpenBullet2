using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class DbJobRepository : DbRepository<JobEntity>, IJobRepository
    {
        public DbJobRepository(ApplicationDbContext context)
            : base(context)
        {

        }

        public async override Task Update(JobEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await base.Update(entity);
        }

        public async override Task Update(IEnumerable<JobEntity> entities)
        {
            foreach (var entity in entities)
            {
                context.Entry(entity).State = EntityState.Modified;
            }

            await base.Update(entities);
        }

        public void Purge()
        {
            context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Jobs)}");
        }
    }
}
