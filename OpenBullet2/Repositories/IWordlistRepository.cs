using OpenBullet2.Entities;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public interface IWordlistRepository
    {
        Task Add(WordlistEntity entity);
        Task Add(WordlistEntity entity, MemoryStream stream);
        Task Delete(WordlistEntity entity, bool deleteFile = true);
        Task<WordlistEntity> Get(int id);
        IQueryable<WordlistEntity> GetAll();
        Task Update(WordlistEntity entity);
    }
}
