using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories
{
    /// <summary>
    /// Stores data to a database.
    /// </summary>
    /// <typeparam name="T">The type of data to store</typeparam>
    public class DbRepository<T> : IRepository<T> where T : Entity
    {
        protected readonly ApplicationDbContext context;
        private readonly SemaphoreSlim semaphore = new(1, 1);

        public DbRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        /// <inheritdoc/>
        public async virtual Task Add(T entity, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            
            try
            {
                context.Add(entity);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async virtual Task Add(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                context.AddRange(entities);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async virtual Task Delete(T entity, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                context.Remove(entity);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async virtual Task Delete(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                context.RemoveRange(entities);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async virtual Task<T> Get(int id, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                return await GetAll()
                    .FirstOrDefaultAsync(e => e.Id == id, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual IQueryable<T> GetAll()
            => context.Set<T>();

        /// <inheritdoc/>
        public async virtual Task Update(T entity, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                context.Update(entity);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async virtual Task Update(IEnumerable<T> entities, CancellationToken cancellationToken = default)
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                context.UpdateRange(entities);
                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public void Attach<TEntity>(TEntity entity) where TEntity : Entity => context.Attach(entity);
    }
}
