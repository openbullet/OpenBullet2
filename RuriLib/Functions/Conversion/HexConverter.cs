using RuriLib.Attributes;
using RuriLib.Extensions;
using System;
using System.Linq;
using System.Text;

namespace RuriLib.Functions.Conversion
{
    public static class HexConverter
    {
        /// <summary>
        /// Converts a <see cref="string"/> <paramref name="str"/> of hexadecimal values to a <see cref="byte[]"/>,
        /// optionally adding a padding to the left if one of the octets is incomplete.
        /// </summary>
        public static byte[] ToByteArray(string str, bool addPadding = true)
        {
            if (str.Contains(" "))
                str = str.Replace(" ", "");

            if (str.Contains("0x"))
                str = str.Replace("0x", "");
            
            if (addPadding)
                str = str.PadLeftToNearestMultiple(2);
                
            return str.SplitInChunks(2, false)
                .Select(hex => Convert.ToByte(hex, 16))
                .ToArray();
        }

        /// <summary>
        /// Converts a <see cref="byte[]"/> to a hex-encoded string.
        /// </summary>
        public static string ToHexString(byte[] bytes)
            => string.Concat(bytes.Select(b => Convert.ToString(b, 16).PadLeft(2, '0')));
    }
}
