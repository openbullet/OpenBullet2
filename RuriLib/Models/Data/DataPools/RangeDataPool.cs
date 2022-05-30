using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Data.DataPools
{
    public class RangeDataPool : DataPool
    {
        public long Start { get; private set; }
        public int Amount { get; private set; }
        public int Step { get; private set; }
        public bool Pad { get; private set; }

        public readonly int POOL_CODE = -3;

        /// <summary>
        /// Creates a DataPool by counting numbers from <paramref name="start"/>, increasing
        /// by <paramref name="step"/> for <paramref name="amount"/> times.
        /// </summary>
        /// <param name="pad">Optionally adds an automatic padding basing on the longest
        /// number's amount of digits.</param>
        /// <example>new DataPool(1, 10, 1, true) will give [01, 02.. 10]</example>
        public RangeDataPool(long start, int amount, int step = 1, bool pad = false, string wordlistType = "Default")
        {
            Start = start;
            Amount = amount;
            Step = step;
            Pad = pad;

            var end = start + step * (amount - 1);
            var maxLength = end.ToString().Length;
            DataList = Range(start, end, step)
                .Select(i => pad ? i.ToString().PadLeft(maxLength, '0') : i.ToString());
            Size = amount;
            WordlistType = wordlistType;
        }

        private IEnumerable<long> Range(long min, long max, int step)
        {
            for (var i = min; i <= max; i += step) yield return i;
        }
    }
}
