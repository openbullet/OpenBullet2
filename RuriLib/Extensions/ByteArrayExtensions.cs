using System;
using System.Collections.Generic;
using System.Text;

namespace RuriLib.Extensions
{
    public static class ByteArrayExtensions
    {
        public static bool AsBool(this byte[] bytes)
            => BitConverter.ToBoolean(bytes, 0);

        public static int AsInt(this byte[] bytes)
            => BitConverter.ToInt32(bytes, 0);

        public static float AsFloat(this byte[] bytes)
            => BitConverter.ToSingle(bytes, 0);

        public static byte[] AsBytes(this byte[] bytes)
            => bytes;

        public static string AsString(this byte[] bytes)
            => Encoding.UTF8.GetString(bytes);

        public static List<string> AsList(this byte[] bytes)
            => new List<string> { bytes.ToString() };

        public static Dictionary<string, string> AsDict(this byte[] bytes)
            => new Dictionary<string, string> { { bytes.ToString(), "" } };
    }
}
