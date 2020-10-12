using System.Collections;

namespace RuriLib.Functions.Files
{
    /// <summary>
    /// Singleton class that manages application-wide file locking to avoid cross thread IO operations on the same file.
    /// </summary>
    public static class FileLocker
    {
        /// <summary>
        /// Maps file names to lockable objects.
        /// </summary>
        public static Hashtable Hashtable = new Hashtable();

        /// <summary>
        /// Gets a lock by file name or creates one if it doesn't exist.
        /// </summary>
        /// <param name="fileName">The name of the file to access</param>
        /// <returns>An object that can be used in a lock statement.</returns>
        public static object GetLock(string fileName)
        {
            if (!Hashtable.ContainsKey(fileName))
            {
                Hashtable.Add(fileName, new object());
            }

            return Hashtable[fileName];
        }
    }
}
