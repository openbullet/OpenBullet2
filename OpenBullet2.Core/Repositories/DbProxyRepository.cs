using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories;

/// <summary>
/// Stores proxies to a database.
/// </summary>
public class DbProxyRepository : DbRepository<ProxyEntity>, IProxyRepository
{
    public DbProxyRepository(ApplicationDbContext context)
        : base(context)
    {
        
    }

    public async override Task UpdateAsync(ProxyEntity entity, CancellationToken cancellationToken = default)
    {
        context.Entry(entity).State = EntityState.Modified;
        await base.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async override Task UpdateAsync(IEnumerable<ProxyEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            context.Entry(entity).State = EntityState.Modified;
        }

        await base.UpdateAsync(entities, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<int> RemoveDuplicatesAsync(int groupId)
    {
        var proxies = await GetAll()
            .Where(p => p.Group.Id == groupId)
            .ToListAsync();

        var duplicates = proxies
            .GroupBy(p => new { p.Type, p.Host, p.Port, p.Username, p.Password })
            .SelectMany(g => g.Skip(1))
            .ToList();
        
        await DeleteAsync(duplicates);

        return duplicates.Count;
    }
}
