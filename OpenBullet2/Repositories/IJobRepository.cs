using OpenBullet2.Entities;

namespace OpenBullet2.Repositories
{
    public interface IJobRepository : IRepository<JobEntity>
    {
        void Purge();
    }
}
