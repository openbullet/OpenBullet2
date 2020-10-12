using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RuriLib
{
    public class GenericComparer<T> : IEqualityComparer<T>
    {
        public bool Equals([AllowNull] T x, [AllowNull] T y)
        {
            if (x == null && y == null) return true;
            else if (x == null || y == null) return false;
            else return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode([DisallowNull] T obj)
            => obj.GetHashCode();
    }
}
