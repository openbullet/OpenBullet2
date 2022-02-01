using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores configs.
    /// </summary>
    public interface IConfigRepository
    {
        /// <summary>
        /// Creates a new config with a given <paramref name="id"/>.
        /// If <paramref name="id"/> is null, a random one will be generated.
        /// </summary>
        Task<Config> Create(string id = null);

        /// <summary>
        /// Deletes a config from the repository.
        /// </summary>
        void Delete(Config config);

        /// <summary>
        /// Retrieves and unpacks a config by ID.
        /// </summary>
        Task<Config> Get(string id);

        /// <summary>
        /// Retrieves and unpacks all configs from the repository.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Config>> GetAll();

        /// <summary>
        /// Retrieves the raw bytes of the OPK config from the repository.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<byte[]> GetBytes(string id);

        /// <summary>
        /// Packs and saves a config to the repository.
        /// </summary>
        Task Save(Config config);

        /// <summary>
        /// Saves a packed config (as a raw bytes stream) to the repository.
        /// </summary>
        Task Upload(Stream stream, string fileName);
    }
}
