using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Extensions
{
    public static class DictionaryOfStringsExtensions
    {
        public static bool AsBool(Dictionary<string, string> _)
            => throw new InvalidCastException("Cannot convert a dictionary of strings to a bool");

        public static int AsInt(Dictionary<string, string> _)
            => throw new InvalidCastException("Cannot convert a dictionary of strings to an int");

        public static float AsFloat(Dictionary<string, string> _)
            => throw new InvalidCastException("Cannot convert a dictionary of strings to a float");

        public static byte[] AsBytes(this Dictionary<string, string> _)
            => throw new InvalidCastException("Cannot convert a dictionary of strings to a byte array");

        public static string AsString(this Dictionary<string, string> dict)
            => "{" + string.Join(", ", dict.AsList().Select(s => $"({s})")) + "}";

        public static List<string> AsList(this Dictionary<string, string> dict)
            => dict.Select(kvp => $"{kvp.Key}, {kvp.Value}").ToList();

        public static Dictionary<string, string> AsDict(this Dictionary<string, string> dict)
            => dict;
    }
}
