using System;

namespace OpenBullet2.Native.Extensions
{
    public static class ObjectExtensions
    {
        public static T AsEnum<T>(this object obj) where T : Enum
            => (T)Enum.Parse(typeof(T), (string)obj);
    }
}
