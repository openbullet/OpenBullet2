using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Extensions;
using System.Collections.Generic;
using System.Threading;
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

        public override async Task<JobEntity> Get(int id, CancellationToken cancellationToken = default)
        {
            var entity = await base.Get(id, cancellationToken).ConfigureAwait(false);
            context.Entry(entity).Reload();
            return entity;
        }

        public async override Task Update(JobEntity entity, CancellationToken cancellationToken = default)
        {
            context.DetachLocal<JobEntity>(entity.Id);
            context.Entry(entity).State = EntityState.Modified;
            await base.Update(entity, cancellationToken).ConfigureAwait(false);
        }

        public async override Task Update(IEnumerable<JobEntity> entities, CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                context.DetachLocal<JobEntity>(entity.Id);
                context.Entry(entity).State = EntityState.Modified;
            }

            await base.Update(entities, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Purge() => context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Jobs)}");
    }
}
