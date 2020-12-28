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
        private readonly object dbLock = new object();

        public DbRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <summary>
        /// Adds an entity and saves changes.
        /// </summary>
        public virtual async Task Add(T entity)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                context.Add(entity);
                await context.SaveChangesAsync();
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }

        /// <summary>
        /// Adds multiple entities and saves changes.
        /// </summary>
        public virtual async Task Add(IEnumerable<T> entities)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                context.AddRange(entities);
                await context.SaveChangesAsync();
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }

        /// <summary>
        /// Deletes an entity and saves changes.
        /// </summary>
        public virtual async Task Delete(T entity)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                context.Remove(entity);
                await context.SaveChangesAsync();
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }

        /// <summary>
        /// Deletes multiple entities and saves changes.
        /// </summary>
        public virtual async Task Delete(IEnumerable<T> entities)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                context.RemoveRange(entities);
                await context.SaveChangesAsync();
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }

        /// <summary>
        /// Gets the entry with the specified id or null if not found.
        /// </summary>
        public virtual async Task<T> Get(int id)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                return await GetAll().FirstOrDefaultAsync(e => e.Id == id);
            }
            finally
            {
                Monitor.Exit(dbLock);
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
        public virtual async Task Update(T entity)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                context.Update(entity);
                await context.SaveChangesAsync();
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }

        /// <summary>
        /// Updates multiple entities and saves changes.
        /// </summary>
        public virtual async Task Update(IEnumerable<T> entities)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                context.UpdateRange(entities);
                await context.SaveChangesAsync();
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }
    }
}
