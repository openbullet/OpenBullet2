using Newtonsoft.Json;
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
    }
}
