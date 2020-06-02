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
        private object dbLock = new object();

        public DbRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

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

        public virtual async Task<T> Get(int id)
        {
            while (!Monitor.TryEnter(dbLock))
                await Task.Delay(10);

            try
            {
                return await GetAll().FirstAsync(e => e.Id == id);
            }
            finally
            {
                Monitor.Exit(dbLock);
            }
        }

        public virtual IQueryable<T> GetAll()
        {
            return context.Set<T>();
        }

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
