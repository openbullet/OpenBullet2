using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;

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
    }
}
