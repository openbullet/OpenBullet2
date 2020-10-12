using RuriLib.Attributes;
using RuriLib.Extensions;
using System;
using System.Linq;
using System.Text;

namespace RuriLib.Functions.Conversion
{
    public static class HexConverter
    {
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

        public static string ToHexString(byte[] bytes)
            => string.Concat(bytes.Select(b => Convert.ToString(b, 16).PadLeft(2, '0')));
    }
}
