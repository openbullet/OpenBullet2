using RuriLib.Models.Data.DataPools;

namespace OpenBullet2.Core.Models.Data
{
    /// <summary>
    /// Options for a <see cref="CombinationsDataPool"/>.
    /// </summary>
    public class CombinationsDataPoolOptions : DataPoolOptions
    {
        /// <summary>
        /// The possible characters that can be in a combination, one after the other without separators.
        /// </summary>
        public string CharSet { get; set; } = "0123456789";

        /// <summary>
        /// The length of the combinations to generate.
        /// </summary>
        public int Length { get; set; } = 4;

        /// <summary>
        /// The Wordlist Type.
        /// </summary>
        public string WordlistType { get; set; } = "Default";
    }
}
