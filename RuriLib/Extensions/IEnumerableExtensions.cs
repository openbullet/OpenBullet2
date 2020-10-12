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
    }
}
