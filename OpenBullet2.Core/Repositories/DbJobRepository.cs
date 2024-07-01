using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories;

/// <summary>
/// Stores jobs to a database.
/// </summary>
public class DbJobRepository : DbRepository<JobEntity>, IJobRepository
{
    public DbJobRepository(ApplicationDbContext context)
        : base(context)
    {

    }

    public override async Task<JobEntity> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await context.Jobs
            .Include(j => j.Owner)
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
        await context.Entry(entity).ReloadAsync(cancellationToken);
        return entity;
    }

    public async override Task UpdateAsync(JobEntity entity, CancellationToken cancellationToken = default)
    {
        context.DetachLocal<JobEntity>(entity.Id);
        context.Entry(entity).State = EntityState.Modified;
        await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async override Task UpdateAsync(IEnumerable<JobEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            context.DetachLocal<JobEntity>(entity.Id);
            context.Entry(entity).State = EntityState.Modified;
        }

        await base.UpdateAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Purge() => context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Jobs)}");
}
