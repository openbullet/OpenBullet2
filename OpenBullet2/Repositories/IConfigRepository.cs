using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public interface IConfigRepository
    {
        Task<Config> Create();
        void Delete(Config config);
        Task<Config> Get(string id);
        Task<List<Config>> GetAll();
        Task<byte[]> GetBytes(string id);
        Task Save(Config config);
        Task Upload(Stream stream);
    }
}
