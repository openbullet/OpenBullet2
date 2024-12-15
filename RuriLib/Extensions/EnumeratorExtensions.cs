using System.Collections.Generic;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IEnumerator{T}"/>.
/// </summary>
public static class EnumeratorExtensions
{
    /// <summary>
    /// Converts an <see cref="IEnumerator{T}"/> to an <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;
        }
    }
}
