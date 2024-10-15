using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RuriLib;

public class GenericComparer<T> : IEqualityComparer<T>
{
    public bool Equals(T? x, T? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }
            
        return x.GetHashCode() == y.GetHashCode();
    }

    public int GetHashCode([DisallowNull] T obj)
        => obj.GetHashCode();
}
