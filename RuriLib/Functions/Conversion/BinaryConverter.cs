using RuriLib.Extensions;
using System;
using System.Linq;

namespace RuriLib.Functions.Conversion
{
    public static class BinaryConverter
    {
        /// <summary>
        /// Converts a <see cref="string"/> <paramref name="str"/> of zeroes and ones to a <see cref="byte[]"/>,
        /// optionally adding a padding to the left if one of the octets is incomplete.
        /// </summary>
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

        /// <summary>
        /// Converts a <see cref="byte[]"/> to a string of ones and zeroes.
        /// </summary>
        public static string ToBinaryString(byte[] bytes)
            => string.Concat(bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }
}
