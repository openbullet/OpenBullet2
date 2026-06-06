using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Files;

/// <summary>
/// Singleton class that manages application-wide file locking to avoid cross thread IO operations on the same file.
/// </summary>
public static class FileLocker
{
    private static readonly Hashtable hashtable = [];

    /// <summary>
    /// Gets a <see cref="RWLock"/> associated to a file name or creates one if it doesn't exist.
    /// </summary>
    /// <param name="fileName">The name of the file to access</param>
    public static RWLock GetHandle(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        if (!hashtable.ContainsKey(fileName))
        {
            hashtable.Add(fileName, new RWLock());
        }

        return (RWLock)(hashtable[fileName] ?? throw new InvalidOperationException("Lock handle cannot be null."));
    }
}

/// <summary>
/// Provides asynchronous read/write locking semantics for a single file handle.
/// </summary>
public class RWLock : IDisposable
{
    private readonly SemaphoreSlim readLock = new(1, 1);
    private readonly SemaphoreSlim writeLock = new(1, 1);

    /// <summary>
    /// Enters the read lock.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the lock is acquired.</returns>
    public Task EnterReadLock(CancellationToken cancellationToken = default) => readLock.WaitAsync(cancellationToken);

    /// <summary>
    /// Enters the write lock.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the lock is acquired.</returns>
    public async Task EnterWriteLock(CancellationToken cancellationToken = default)
    {
        await readLock.WaitAsync(cancellationToken);
        await writeLock.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Exits the read lock.
    /// </summary>
    public void ExitReadLock() => readLock.Release();

    /// <summary>
    /// Exits the write lock.
    /// </summary>
    public void ExitWriteLock()
    {
        readLock.Release();
        writeLock.Release();
    }

    /// <summary>
    /// Disposes the underlying semaphores.
    /// </summary>
    public void Dispose()
    {
        readLock.Dispose();
        writeLock.Dispose();
    }
}
