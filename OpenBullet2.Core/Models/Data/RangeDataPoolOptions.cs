using RuriLib.Models.Data.DataPools;

namespace OpenBullet2.Core.Models.Data
{
    /// <summary>
    /// Options for a <see cref="RangeDataPool"/>.
    /// </summary>
    public class RangeDataPoolOptions : DataPoolOptions
    {
        /// <summary>
        /// The start of the range.
        /// </summary>
        public long Start { get; set; } = 0;

        /// <summary>
        /// The length of the range.
        /// </summary>
        public int Amount { get; set; } = 100;

        /// <summary>
        /// The entity of the interval between elements.
        /// </summary>
        public int Step { get; set; } = 1;

        /// <summary>
        /// Whether to pad numbers with zeroes basing on the number
        /// of digits of the biggest number to generate.
        /// </summary>
        public bool Pad { get; set; } = false;

        /// <summary>
        /// The Wordlist Type.
        /// </summary>
        public string WordlistType { get; set; } = "Default";
    }
}
