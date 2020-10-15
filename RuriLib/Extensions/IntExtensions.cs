using System;
using System.Collections.Generic;

namespace RuriLib.Extensions
{
    public static class IntExtensions
    {
        public static bool AsBool(this int i)
            => i == 1;

        public static int AsInt(this int i)
            => i;

        public static float AsFloat(this int i)
            => Convert.ToSingle(i);

        public static byte[] AsBytes(this int i)
            => BitConverter.GetBytes(i);

        public static string AsString(this int i)
            => i.ToString();

        public static List<string> AsList(this int i)
            => new List<string> { i.AsString() };

        public static Dictionary<string, string> AsDict(this int i)
            => new Dictionary<string, string> { { i.AsString(), "" } };
    }
}
