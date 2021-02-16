using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
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

        /// <summary>
        /// Adds a wordlist that already exists on disk.
        /// </summary>
        public async Task Add(WordlistEntity entity)
        {
            // Save it to the DB
            context.Add(entity);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds a wordlist and writes its content to disk.
        /// </summary>
        public async Task Add(WordlistEntity entity, MemoryStream stream)
        {
            // Generate a unique filename
            entity.FileName = Path.Combine(baseFolder, $"{Guid.NewGuid()}.txt").Replace('\\', '/');

            // Create the file on disk
            await File.WriteAllBytesAsync(entity.FileName, stream.ToArray());

            // Count the amount of lines
            entity.Total = File.ReadLines(entity.FileName).Count();

            await Add(entity);
        }

        public IQueryable<WordlistEntity> GetAll()
            => context.Wordlists;

        public async Task<WordlistEntity> Get(int id)
            => await GetAll().FirstOrDefaultAsync(e => e.Id == id);

        public async Task Update(WordlistEntity entity)
        {
            context.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task Delete(WordlistEntity entity, bool deleteFile = true)
        {
            if (deleteFile && File.Exists(entity.FileName))
                File.Delete(entity.FileName);

            context.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
