using System.Collections.Generic;

namespace RuriLib.Extensions
{
    public static class EnumeratorExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerator{T}"/> to an <see cref="IEnumerable{T}"/>.
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }
}
