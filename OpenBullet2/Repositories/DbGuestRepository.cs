using OpenBullet2.Entities;

namespace OpenBullet2.Repositories
{
    public class DbGuestRepository : DbRepository<GuestEntity>, IGuestRepository
    {
        public DbGuestRepository(ApplicationDbContext context)
            : base(context)
        {

        }
    }
}
