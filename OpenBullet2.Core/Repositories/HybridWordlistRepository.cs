using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories
{
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
        public async Task Add(WordlistEntity entity)
        {
            // Save it to the DB
            context.Add(entity);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task Add(WordlistEntity entity, MemoryStream stream)
        {
            // Generate a unique filename
            var path = Path.Combine(baseFolder, $"{Guid.NewGuid()}.txt");
            entity.FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? path.Replace('/', '\\')
                : path.Replace('\\', '/');

            // Create the file on disk
            await File.WriteAllBytesAsync(entity.FileName, stream.ToArray());

            // Count the amount of lines
            entity.Total = File.ReadLines(entity.FileName).Count();

            await Add(entity);
        }

        /// <inheritdoc/>
        public IQueryable<WordlistEntity> GetAll()
            => context.Wordlists;

        /// <inheritdoc/>
        public async Task<WordlistEntity> Get(int id)
            => await GetAll().FirstOrDefaultAsync(e => e.Id == id);

        /// <inheritdoc/>
        public async Task Update(WordlistEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            context.Update(entity);
            await context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task Delete(WordlistEntity entity, bool deleteFile = false)
        {
            if (deleteFile && File.Exists(entity.FileName))
                File.Delete(entity.FileName);

            context.Remove(entity);
            await context.SaveChangesAsync();
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
}
