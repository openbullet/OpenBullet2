using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Extensions
{
    public static class DictionaryOfStringsExtensions
    {
        public static string AsString(this Dictionary<string, string> dict)
            => "{" + string.Join(", ", dict.AsList().Select(s => $"({s})")) + "}";

        public static List<string> AsList(this Dictionary<string, string> dict)
            => dict.Select(kvp => $"{kvp.Key}, {kvp.Value}").ToList();

        public static Dictionary<string, string> AsDict(this Dictionary<string, string> dict)
            => dict;
    }
}
