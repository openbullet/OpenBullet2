using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Data.DataPools
{
    public class RangeDataPool : DataPool
    {
        public int Start { get; private set; }
        public int Amount { get; private set; }
        public int Step { get; private set; }
        public bool Pad { get; private set; }

        /// <summary>
        /// Creates a DataPool by counting numbers from <paramref name="start"/>, increasing
        /// by <paramref name="step"/> for <paramref name="amount"/> times.
        /// </summary>
        /// <param name="pad">Optionally adds an automatic padding basing on the longest
        /// number's amount of digits.</param>
        /// <example>new DataPool(1, 10, 1, true) will give [01, 02.. 10]</example>
        public RangeDataPool(int start, int amount, int step = 1, bool pad = false, string wordlistType = "Default")
        {
            Start = start;
            Amount = amount;
            Step = step;
            Pad = pad;

            int end = start + step * (amount - 1);
            int maxLength = end.ToString().Length;
            DataList = Range(start, end, step)
                .Select(i => pad ? i.ToString().PadLeft(maxLength, '0') : i.ToString());
            Size = amount;
            WordlistType = wordlistType;
        }

        private IEnumerable<int> Range(int min, int max, int step)
        {
            for (int i = min; i <= max; i += step) yield return i;
        }
    }
}
