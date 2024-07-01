using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories;

/// <summary>
/// Stores wordlists to the disk and the database. Files are stored on disk while
/// metadata is stored in a database.
/// </summary>
public class HybridWordlistRepository : IWordlistRepository
{
    private readonly string baseFolder;
    private readonly ApplicationDbContext context;

    public HybridWordlistRepository(ApplicationDbContext context, string baseFolder)
    {
        this.context = context;
        this.baseFolder = baseFolder;
        Directory.CreateDirectory(baseFolder);
    }

    /// <inheritdoc/>
    public async Task AddAsync(WordlistEntity entity, CancellationToken cancellationToken = default)
    {
        // Save it to the DB
        context.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task AddAsync(WordlistEntity entity, MemoryStream stream,
        CancellationToken cancellationToken = default)
    {
        // Generate a unique filename
        var path = Path.Combine(baseFolder, $"{Guid.NewGuid()}.txt");
        entity.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? path.Replace('/', '\\')
            : path.Replace('\\', '/');

        // Create the file on disk
        await File.WriteAllBytesAsync(entity.FileName, stream.ToArray(),
            cancellationToken);

        // Count the amount of lines
        entity.Total = File.ReadLines(entity.FileName).Count();

        await AddAsync(entity);
    }

    /// <inheritdoc/>
    public IQueryable<WordlistEntity> GetAll()
        => context.Wordlists;

    /// <inheritdoc/>
    public async Task<WordlistEntity> GetAsync(
        int id, CancellationToken cancellationToken = default)
        => await GetAll().Include(w => w.Owner)
        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken: cancellationToken)
        .ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task UpdateAsync(WordlistEntity entity, CancellationToken cancellationToken = default)
    {
        context.Entry(entity).State = EntityState.Modified;
        context.Update(entity);
        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(WordlistEntity entity, bool deleteFile = false,
        CancellationToken cancellationToken = default)
    {
        if (deleteFile && File.Exists(entity.FileName))
            File.Delete(entity.FileName);

        context.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public void Purge() => _ = context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Wordlists)}");

    /// <inheritdoc/>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        context?.Dispose();
    }
}
