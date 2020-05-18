using OpenBullet2.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public interface IRepository<T> where T : Entity
    {
        // CREATE
        Task Add(T entity);
        Task Add(IEnumerable<T> entities);

        // READ
        Task<T> Get(int id);
        IQueryable<T> GetAll();

        // UPDATE
        Task Update(T entity);
        Task Update(IEnumerable<T> entities);

        // DELETE
        Task Delete(T entity);
        Task Delete(IEnumerable<T> entities);
    }
}
