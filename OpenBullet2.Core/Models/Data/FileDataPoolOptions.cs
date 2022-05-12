using RuriLib.Models.Data.DataPools;
using System.Runtime.InteropServices;

namespace OpenBullet2.Core.Models.Data
{
    /// <summary>
    /// Options for a <see cref="FileDataPool"/>.
    /// </summary>
    public class FileDataPoolOptions : DataPoolOptions
    {
        private string fileName = null;

        /// <summary>
        /// The path to the file on disk.
        /// </summary>
        public string FileName
        { 
            get => fileName;
            set
            {
                // Double quotes in file names are not allowed in Windows, but they are included
                // at the start and end of the file path if you copy/paste it from some programs,
                // so we need to remove them, otherwise it will not find the file.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Remove the double quotes from the file
                    fileName = value.Replace("\"", "");
                }
                else
                {
                    fileName = value;
                }
            }
        }

        /// <summary>
        /// The Wordlist Type.
        /// </summary>
        public string WordlistType { get; set; } = "Default";
    }
}
