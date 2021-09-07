using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores jobs to a database.
    /// </summary>
    public class DbJobRepository : DbRepository<JobEntity>, IJobRepository
    {
        public DbJobRepository(ApplicationDbContext context)
            : base(context)
        {

        }

        public override async Task<JobEntity> Get(int id)
        {
            var entity = await base.Get(id);
            context.Entry(entity).Reload();
            return entity;
        }

        public async override Task Update(JobEntity entity)
        {
            context.DetachLocal<JobEntity>(entity.Id);
            context.Entry(entity).State = EntityState.Modified;
            await base.Update(entity);
        }

        public async override Task Update(IEnumerable<JobEntity> entities)
        {
            foreach (var entity in entities)
            {
                context.DetachLocal<JobEntity>(entity.Id);
                context.Entry(entity).State = EntityState.Modified;
            }

            await base.Update(entities);
        }

        /// <inheritdoc/>
        public void Purge() => context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Jobs)}");
    }
}
