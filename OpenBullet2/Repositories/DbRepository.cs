using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class DbRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly ApplicationDbContext context;

        public DbRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public virtual async Task Add(T entity)
        {
            context.Add(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task Add(IEnumerable<T> entities)
        {
            context.AddRange(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task Delete(T entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task Delete(IEnumerable<T> entities)
        {
            context.RemoveRange(entities);
            await context.SaveChangesAsync();
        }

        public virtual async Task<T> Get(int id)
        {
            return await GetAll().FirstAsync(e => e.Id == id);
        }

        public virtual IQueryable<T> GetAll()
        {
            return context.Set<T>();
        }

        public virtual async Task Update(T entity)
        {
            context.Update(entity);
            await context.SaveChangesAsync();
        }

        public virtual async Task Update(IEnumerable<T> entities)
        {
            context.UpdateRange(entities);
            await context.SaveChangesAsync();
        }
    }
}
