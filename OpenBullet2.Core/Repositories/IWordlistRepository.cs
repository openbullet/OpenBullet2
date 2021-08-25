using OpenBullet2.Core.Entities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores wordlists.
    /// </summary>
    public interface IWordlistRepository : IDisposable
    {
        /// <summary>
        /// Adds an <paramref name="entity"/> to the repository.
        /// </summary>
        Task Add(WordlistEntity entity);

        /// <summary>
        /// Adds an <paramref name="entity"/> to the repository and creates the file as well
        /// by reading it from a raw <paramref name="stream"/>.
        /// </summary>
        Task Add(WordlistEntity entity, MemoryStream stream);

        /// <summary>
        /// Deletes an <paramref name="entity"/> from the repository.
        /// </summary>
        /// <param name="deleteFile">Whether to delete the file as well</param>
        Task Delete(WordlistEntity entity, bool deleteFile = false);

        /// <summary>
        /// Gets an entity from the repository by <paramref name="id"/>.
        /// </summary>
        Task<WordlistEntity> Get(int id);

        /// <summary>
        /// Gets an <see cref="IQueryable"/> of all entities for further filtering.
        /// </summary>
        /// <returns></returns>
        IQueryable<WordlistEntity> GetAll();

        /// <summary>
        /// Updates an <paramref name="entity"/> in the repository.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task Update(WordlistEntity entity);

        /// <summary>
        /// Deletes all wordlists from the repository.
        /// </summary>
        void Purge();
    }
}
