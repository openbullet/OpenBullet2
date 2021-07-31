using RuriLib.Models.Data.DataPools;

namespace OpenBullet2.Core.Models.Data
{
    /// <summary>
    /// Options for a <see cref="FileDataPool"/>.
    /// </summary>
    public class FileDataPoolOptions : DataPoolOptions
    {
        /// <summary>
        /// The path to the file on disk.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The Wordlist Type.
        /// </summary>
        public string WordlistType { get; set; } = "Default";
    }
}
