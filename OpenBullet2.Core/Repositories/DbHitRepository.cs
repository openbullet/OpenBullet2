using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores hits to a database.
    /// </summary>
    public class DbHitRepository : DbRepository<HitEntity>, IHitRepository
    {
        public DbHitRepository(ApplicationDbContext context)
            : base(context)
        {

        }

        /// <inheritdoc/>
        public void Purge() => _ = context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Hits)}");

        public async override Task Update(HitEntity entity)
        {
            context.DetachLocal<HitEntity>(entity.Id);
            context.Entry(entity).State = EntityState.Modified;
            await base.Update(entity);
        }

        public async override Task Update(IEnumerable<HitEntity> entities)
        {
            foreach (var entity in entities)
            {
                context.DetachLocal<HitEntity>(entity.Id);
                context.Entry(entity).State = EntityState.Modified;
            }

            await base.Update(entities);
        }
    }
}
