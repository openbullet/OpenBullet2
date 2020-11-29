using System;
using System.Collections.Generic;

namespace RuriLib.Extensions
{
    public static class BoolExtensions
    {
        public static bool AsBool(this bool b)
            => b;

        public static int AsInt(this bool b)
            => Convert.ToInt32(b);

        public static float AsFloat(this bool b)
            => Convert.ToSingle(b);

        public static byte[] AsBytes(this bool b)
            => BitConverter.GetBytes(b);

        public static string AsString(this bool b)
            => b.ToString();

        public static List<string> AsList(this bool b)
            => new List<string> { b.ToString() };

        public static Dictionary<string, string> AsDict(this bool b)
            => new Dictionary<string, string> { { b.ToString(), "" } };
    }
}
