using System.Collections;

namespace RuriLib.Functions.Files
{
    /// <summary>
    /// Singleton class that manages application-wide file locking to avoid cross thread IO operations on the same file.
    /// </summary>
    public static class FileLocker
    {
        private static readonly Hashtable hashtable = new();

        /// <summary>
        /// Gets a lockable handle associated to a file name or creates one if it doesn't exist.
        /// </summary>
        /// <param name="fileName">The name of the file to access</param>
        public static object GetHandle(string fileName)
        {
            if (!hashtable.ContainsKey(fileName))
            {
                hashtable.Add(fileName, new object());
            }

            return hashtable[fileName];
        }
    }
}
