using System;

namespace RuriLib.Extensions
{
    public static class DoubleExtensions
    {
        public static float ToSingle(this double d)
            => Convert.ToSingle(d);

        public static int ToInt(this double d)
            => Convert.ToInt32(d);
    }
}
