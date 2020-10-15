using System;
using System.Collections.Generic;
using System.Globalization;

namespace RuriLib.Extensions
{
    public static class FloatExtensions
    {
        public static bool AsBool(this float f)
            => f == 1f;

        public static int AsInt(this float f)
            => (int)f;

        public static float AsFloat(this float f)
            => f;

        public static byte[] AsBytes(this float f)
            => BitConverter.GetBytes(f);

        public static string AsString(this float f)
            => f.ToString(CultureInfo.InvariantCulture);

        public static List<string> AsList(this float f)
            => new List<string> { f.AsString() };

        public static Dictionary<string, string> AsDict(this float f)
            => new Dictionary<string, string> { { f.AsString(), "" } };
    }
}
