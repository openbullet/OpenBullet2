using OpenBullet2.Entities;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public interface IHitRepository : IRepository<HitEntity>
    {
        void Purge();
    }
}
