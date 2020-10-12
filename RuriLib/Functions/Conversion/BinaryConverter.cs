using RuriLib.Extensions;
using System;
using System.Linq;

namespace RuriLib.Functions.Conversion
{
    public static class BinaryConverter
    {
        public static byte[] ToByteArray(string str, bool addPadding = true)
        {
            if (str.Contains(" "))
                str = str.Replace(" ", "");

            if (addPadding)
                str = str.PadLeftToNearestMultiple(8);

            return str.SplitInChunks(8, false)
                .Select(octet => Convert.ToByte(octet, 2))
                .ToArray();
        }

        public static string ToBinaryString(byte[] bytes)
            => string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }
}
