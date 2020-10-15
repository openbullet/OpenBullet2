using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Extensions
{
    public static class ListOfStringsExtensions
    {
        public static bool AsBool(this List<string> list)
            => list.First().AsBool();

        public static int AsInt(this List<string> list)
            => list.First().AsInt();

        public static float AsFloat(this List<string> list)
            => list.First().AsFloat();

        public static byte[] AsBytes(this List<string> list)
            => list.First().AsBytes();

        public static string AsString(this List<string> list)
            => "[" + string.Join(", ", list) + "]";

        public static List<string> AsList(this List<string> list)
            => list;

        public static Dictionary<string, string> AsDict(this List<string> list)
            => list.Count == list.Distinct().Count()
            ? list.ToDictionary(l => l, l => "")
            : throw new InvalidCastException("When converting a list to dictionary, all keys must be unique");
    }
}
