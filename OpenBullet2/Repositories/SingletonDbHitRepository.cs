using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class SingletonDbHitRepository
    {
        private readonly ApplicationDbContext context;

        public SingletonDbHitRepository(DbContextOptions<ApplicationDbContext> dbOptions)
        {
            context = new ApplicationDbContext(dbOptions);
        }

        public async Task Store(HitEntity hit)
        {
            context.Hits.Add(hit);
            await context.SaveChangesAsync();
        }
    }
}
