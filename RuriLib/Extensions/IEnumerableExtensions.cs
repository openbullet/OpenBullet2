using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Makes a deep copy of an <see cref="IEnumerable{T}"/> by serializing and deserializing it.
        /// </summary>
        public static IEnumerable<T> Clone<T>(this IEnumerable<T> enumerable)
            => enumerable.Select(item => JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(item)));

        /// <summary>
        /// Takes the distinct values of an <see cref="IEnumerable{T}"/> without enumerating it.
        /// </summary>
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
