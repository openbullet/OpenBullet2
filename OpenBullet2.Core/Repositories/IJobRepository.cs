using OpenBullet2.Core.Entities;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores jobs.
    /// </summary>
    public interface IJobRepository : IRepository<JobEntity>
    {
        /// <summary>
        /// Deletes all jobs from the repository.
        /// </summary>
        void Purge();
    }
}
