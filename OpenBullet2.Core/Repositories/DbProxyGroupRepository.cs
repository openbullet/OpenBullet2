using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories;

/// <summary>
/// Stores proxy groups to a database.
/// </summary>
public class DbProxyGroupRepository(ApplicationDbContext context) : DbRepository<ProxyGroupEntity>(context), IProxyGroupRepository
{

    /// <inheritdoc/>
    public override async Task<ProxyGroupEntity> GetAsync(int id, CancellationToken cancellationToken = default)
        => (await GetAll().Include(w => w.Owner)
        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
        .ConfigureAwait(false))!;
}
