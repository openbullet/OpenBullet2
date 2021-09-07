using RuriLib.Models.Data.DataPools;

namespace OpenBullet2.Core.Models.Data
{
    /// <summary>
    /// Options for a <see cref="WordlistDataPool"/>.
    /// </summary>
    public class WordlistDataPoolOptions : DataPoolOptions
    {
        /// <summary>
        /// The ID of the Wordlist in the repository.
        /// </summary>
        public int WordlistId { get; set; } = -1;
    }
}
