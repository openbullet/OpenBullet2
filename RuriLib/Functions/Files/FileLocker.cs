using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Files
{
    /// <summary>
    /// Singleton class that manages application-wide file locking to avoid cross thread IO operations on the same file.
    /// </summary>
    public static class FileLocker
    {
        private static readonly Hashtable hashtable = new();

        /// <summary>
        /// Gets a <see cref="RWLock"/> associated to a file name or creates one if it doesn't exist.
        /// </summary>
        /// <param name="fileName">The name of the file to access</param>
        public static RWLock GetHandle(string fileName)
        {
            if (!hashtable.ContainsKey(fileName))
            {
                hashtable.Add(fileName, new RWLock());
            }

            return (RWLock)hashtable[fileName];
        }
    }

    public class RWLock : IDisposable
    {
        private readonly SemaphoreSlim readLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim writeLock = new SemaphoreSlim(1, 1);

        public Task EnterReadLock(CancellationToken cancellationToken = default) => readLock.WaitAsync(cancellationToken);

        public async Task EnterWriteLock(CancellationToken cancellationToken = default)
        {
            await readLock.WaitAsync(cancellationToken);
            await writeLock.WaitAsync(cancellationToken);
        }

        public void ExitReadLock() => readLock.Release();

        public void ExitWriteLock()
        {
            readLock.Release();
            writeLock.Release();
        }

        public void Dispose()
        {
            readLock?.Dispose();
            writeLock?.Dispose();
        }
    }
}
