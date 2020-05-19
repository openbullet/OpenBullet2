using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class DbHitRepository : DbRepository<HitEntity>, IHitRepository
    {
        public DbHitRepository(ApplicationDbContext context) 
            : base(context)
        {

        }

        public void Purge()
        {
            context.Database.ExecuteSqlRaw($"DELETE FROM {nameof(ApplicationDbContext.Hits)}");
        }
    }
}
