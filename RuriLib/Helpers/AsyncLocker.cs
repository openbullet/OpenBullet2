using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers;

/// <summary>
/// Provides keyed asynchronous locking across the process.
/// </summary>
public class AsyncLocker : IDisposable
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> semaphores = new();

    /// <summary>
    /// Acquires a lock for the given key.
    /// </summary>
    /// <param name="key">The lock key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the lock is acquired.</returns>
    public Task Acquire(string key, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(key);

        var semaphore = semaphores.GetOrAdd(key, static _ => new SemaphoreSlim(1, 1));
        return semaphore.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Acquires a lock for the given type and method pair.
    /// </summary>
    /// <param name="classType">The type participating in the lock key.</param>
    /// <param name="methodName">The method name participating in the lock key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the lock is acquired.</returns>
    public Task Acquire(Type classType, string methodName, CancellationToken cancellationToken = default)
        => Acquire(CombineTypes(classType, methodName), cancellationToken);

    /// <summary>
    /// Releases a lock for the given key.
    /// </summary>
    /// <param name="key">The lock key.</param>
    public void Release(string key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!semaphores.TryGetValue(key, out var semaphore))
        {
            throw new InvalidOperationException($"No semaphore exists for key '{key}'.");
        }

        semaphore.Release();
    }

    /// <summary>
    /// Releases a lock for the given type and method pair.
    /// </summary>
    /// <param name="classType">The type participating in the lock key.</param>
    /// <param name="methodName">The method name participating in the lock key.</param>
    public void Release(Type classType, string methodName) => Release(CombineTypes(classType, methodName));

    private static string CombineTypes(Type classType, string methodName)
    {
        ArgumentNullException.ThrowIfNull(classType);
        ArgumentNullException.ThrowIfNull(methodName);

        return $"{classType.FullName}.{methodName}";
    }

    /// <summary>
    /// Disposes the underlying semaphores.
    /// </summary>
    public void Dispose()
    {
        foreach (var semaphore in semaphores.Values)
        {
            semaphore.Dispose();
        }
    }
}
