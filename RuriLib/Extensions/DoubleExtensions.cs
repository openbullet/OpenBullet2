using System;

namespace RuriLib.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="double"/> type.
/// </summary>
public static class DoubleExtensions
{
    /// <summary>
    /// Converts a <see cref="double"/> to a <see cref="float"/>.
    /// </summary>
    public static float ToSingle(this double d)
        => Convert.ToSingle(d);

    /// <summary>
    /// Converts a <see cref="double"/> to an <see cref="int"/>.
    /// </summary>
    public static int ToInt(this double d)
        => Convert.ToInt32(d);
}
