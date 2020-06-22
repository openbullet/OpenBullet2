using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;

namespace OpenBullet2
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<ProxyEntity> Proxies { get; set; }
        public DbSet<ProxyGroupEntity> ProxyGroups { get; set; }
        public DbSet<WordlistEntity> Wordlists { get; set; }
        public DbSet<JobEntity> Jobs { get; set; }
        public DbSet<RecordEntity> Records { get; set; }
        public DbSet<HitEntity> Hits { get; set; }
        public DbSet<GuestEntity> Guests { get; set; }
    }
}
