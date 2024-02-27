using OpenBullet2.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Repositories;

/// <summary>
/// Stores data.
/// </summary>
/// <typeparam name="T">The type of data to store</typeparam>
public interface IRepository<T> where T : Entity
{
    // ------
    // CREATE
    // ------

    /// <summary>
    /// Adds an <paramref name="entity"/> to the repository.
    /// </summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple <paramref name="entities"/> to the repository.
    /// </summary>
    Task AddAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    // ----
    // READ
    // ----

    /// <summary>
    /// Gets an entity by <paramref name="id"/>. Returns null if not found.
    /// </summary>
    Task<T> GetAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an <see cref="IQueryable{T}"/> of all entities in the repository for further filtering.
    /// </summary>
    IQueryable<T> GetAll();

    // ------
    // UPDATE
    // ------

    /// <summary>
    /// Updates an <paramref name="entity"/> in the repository.
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates multiple <paramref name="entities"/> in the repository.
    /// </summary>
    Task UpdateAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    // ------
    // DELETE
    // ------

    /// <summary>
    /// Deletes an <paramref name="entity"/> from the repository.
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes multiple <paramref name="entities"/> from the repository.
    /// </summary>
    Task DeleteAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attaches to a given entity so that EF doesn't try to create a new one
    /// in a one to many relationship.
    /// </summary>
    public void Attach<TEntity>(TEntity entity) where TEntity : Entity;
}
