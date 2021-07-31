using RuriLib.Models.Data.DataPools;

namespace OpenBullet2.Core.Models.Data
{
    /// <summary>
    /// Options for an <see cref="InfiniteDataPool"/>.
    /// </summary>
    public class InfiniteDataPoolOptions : DataPoolOptions
    {
        /// <summary>
        /// The Wordlist Type.
        /// </summary>
        public string WordlistType { get; set; } = "Default";
    }
}
