using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories;

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
    public async Task PurgeAsync() => await context.Database
        .ExecuteSqlRawAsync($"DELETE FROM {nameof(ApplicationDbContext.Hits)}");

    /// <inheritdoc/>
    public async Task<long> CountAsync() => await context.Hits.CountAsync();

    public async override Task UpdateAsync(HitEntity entity, CancellationToken cancellationToken = default)
    {
        context.DetachLocal<HitEntity>(entity.Id);
        context.Entry(entity).State = EntityState.Modified;
        await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async override Task UpdateAsync(IEnumerable<HitEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            context.DetachLocal<HitEntity>(entity.Id);
            context.Entry(entity).State = EntityState.Modified;
        }

        await base.UpdateAsync(entities, cancellationToken).ConfigureAwait(false);
    }
}
