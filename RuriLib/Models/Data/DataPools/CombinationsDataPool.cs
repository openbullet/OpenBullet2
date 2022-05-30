using System;
using System.Linq;

namespace RuriLib.Models.Data.DataPools
{
    public class CombinationsDataPool : DataPool
    {
        public string CharSet { get; private set; }
        public int Length { get; private set; }

        public readonly int POOL_CODE = -4;

        /// <summary>
        /// Creates a DataPool by generating all the possible combinations of a string.
        /// </summary>
        /// <param name="charSet">The allowed character set (one after the other like in the string "abcdef")</param>
        /// <param name="length">The length of the output combinations</param>
        public CombinationsDataPool(string charSet, int length, string wordlistType = "Default")
        {
            CharSet = charSet;
            Length = length;

            DataList = charSet.Select(x => x.ToString());
            for (var i = 0; i < length - 1; i++)
                DataList = DataList.SelectMany(x => charSet, (x, y) => x + y);

            var sizeDouble = Math.Pow(charSet.Length, length);
            Size = sizeDouble < long.MaxValue ? (long)sizeDouble : long.MaxValue;
            WordlistType = wordlistType;
        }
    }
}
