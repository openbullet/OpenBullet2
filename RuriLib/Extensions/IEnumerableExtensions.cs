using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> enumerable)
            => enumerable.Select(item => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(item)));

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
