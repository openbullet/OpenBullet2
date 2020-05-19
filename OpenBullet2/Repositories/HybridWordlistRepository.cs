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
        private readonly ApplicationDbContext context;

        public HybridWordlistRepository(ApplicationDbContext context)
        {
            this.context = context;
            Directory.CreateDirectory("Wordlists");
        }

        public async Task Add(WordlistEntity entity)
        {
            // Save it to the DB
            context.Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task Add(WordlistEntity entity, MemoryStream stream)
        {
            // Generate a unique filename
            entity.FileName = $"{Guid.NewGuid()}.txt";

            // Create the file on disk
            await File.WriteAllBytesAsync(GetFileName(entity), stream.ToArray());

            // Count the amount of lines
            entity.Total = File.ReadLines(GetFileName(entity)).Count();

            await Add(entity);
        }

        public IQueryable<WordlistEntity> GetAll()
        {
            return context.Wordlists;
        }

        public async Task<WordlistEntity> Get(int id)
        {
            return await GetAll().FirstAsync(e => e.Id == id);
        }

        public async Task Update(WordlistEntity entity)
        {
            context.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task Delete(WordlistEntity entity, bool deleteFile = true)
        {
            if (deleteFile && File.Exists(GetFileName(entity)))
                File.Delete(GetFileName(entity));

            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        private string GetFileName(WordlistEntity wordlist)
            => GetFileName(wordlist.FileName);

        private string GetFileName(string fileName)
            => $"Wordlists/{fileName}";
    }
}
