using OpenBullet2.Core.Entities;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores guests to a database.
    /// </summary>
    public class DbGuestRepository : DbRepository<GuestEntity>, IGuestRepository
    {
        public DbGuestRepository(ApplicationDbContext context)
            : base(context)
        {

        }
    }
}
