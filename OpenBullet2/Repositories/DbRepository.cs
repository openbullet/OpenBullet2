using Microsoft.EntityFrameworkCore;
using OpenBullet2.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class DbRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly ApplicationDbContext context;
        private readonly SemaphoreSlim semaphore = new(1, 1);

        public DbRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adds an entity and saves changes.
        /// </summary>
        public async virtual Task Add(T entity)
        {
            await semaphore.WaitAsync();
            
            try
            {
                context.Add(entity);
                await context.SaveChangesAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Adds multiple entities and saves changes.
        /// </summary>
        public async virtual Task Add(IEnumerable<T> entities)
        {
            await semaphore.WaitAsync();

            try
            {
                context.AddRange(entities);
                await context.SaveChangesAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Deletes an entity and saves changes.
        /// </summary>
        public async virtual Task Delete(T entity)
        {
            await semaphore.WaitAsync();

            try
            {
                context.Remove(entity);
                await context.SaveChangesAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Deletes multiple entities and saves changes.
        /// </summary>
        public async virtual Task Delete(IEnumerable<T> entities)
        {
            await semaphore.WaitAsync();

            try
            {
                context.RemoveRange(entities);
                await context.SaveChangesAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Gets the entry with the specified id or null if not found.
        /// </summary>
        public async virtual Task<T> Get(int id)
        {
            await semaphore.WaitAsync();

            try
            {
                return await GetAll().FirstOrDefaultAsync(e => e.Id == id);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> of all entries for further filtering.
        /// </summary>
        public virtual IQueryable<T> GetAll()
            => context.Set<T>();

        /// <summary>
        /// Updates the given entity and saves changes.
        /// </summary>
        public async virtual Task Update(T entity)
        {
            await semaphore.WaitAsync();

            try
            {
                context.Update(entity);
                await context.SaveChangesAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Updates multiple entities and saves changes.
        /// </summary>
        public async virtual Task Update(IEnumerable<T> entities)
        {
            await semaphore.WaitAsync();

            try
            {
                context.UpdateRange(entities);
                await context.SaveChangesAsync();
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
