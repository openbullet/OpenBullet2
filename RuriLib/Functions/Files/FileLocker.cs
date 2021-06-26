using System.Collections;
using System.Threading;

namespace RuriLib.Functions.Files
{
    /// <summary>
    /// Singleton class that manages application-wide file locking to avoid cross thread IO operations on the same file.
    /// </summary>
    public static class FileLocker
    {
        private static readonly Hashtable hashtable = new();

        /// <summary>
        /// Gets a <see cref="ReaderWriterLockSlim"/> associated to a file name or creates one if it doesn't exist.
        /// </summary>
        /// <param name="fileName">The name of the file to access</param>
        public static ReaderWriterLockSlim GetHandle(string fileName)
        {
            if (!hashtable.ContainsKey(fileName))
            {
                hashtable.Add(fileName, new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion));
            }

            return (ReaderWriterLockSlim)hashtable[fileName];
        }
    }
}
