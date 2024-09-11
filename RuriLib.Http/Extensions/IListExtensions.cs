using System.Collections.Generic;

namespace RuriLib.Http.Extensions;

internal static class ListExtensions
{
    public static void Add(this IList<KeyValuePair<string, string>> list, string key, object value)
        => list.Add(new KeyValuePair<string, string>(key, value.ToString()!));
}
