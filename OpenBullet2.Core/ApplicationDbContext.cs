using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;

namespace OpenBullet2.Core
{
    /// <summary>
    /// The <see cref="DbContext"/> for the OpenBullet 2 core domain.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
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
