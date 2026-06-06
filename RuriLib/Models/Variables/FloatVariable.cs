using System;
using System.Collections.Generic;
using System.Globalization;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents a floating-point variable.
/// </summary>
public class FloatVariable : Variable
{
    private readonly double value;

    /// <summary>
    /// Creates a floating-point variable.
    /// </summary>
    /// <param name="value">The floating-point value.</param>
    public FloatVariable(double value)
    {
        this.value = value;
        Type = VariableType.Float;
    }

    /// <inheritdoc />
    public override string AsString() => value.ToString(CultureInfo.InvariantCulture);

    /// <inheritdoc />
    public override int AsInt() => Convert.ToInt32(value);

    /// <inheritdoc />
    public override long AsLong() => Convert.ToInt64(value);

    /// <inheritdoc />
    public override bool AsBool() => value switch
    {
        0 => false,
        1 => true,
        _ => throw new InvalidCastException()
    };

    /// <inheritdoc />
    public override byte[] AsByteArray() => BitConverter.GetBytes(value);

    /// <inheritdoc />
    public override float AsFloat() => Convert.ToSingle(value);

    /// <inheritdoc />
    public override double AsDouble() => value;

    /// <inheritdoc />
    public override List<string> AsListOfStrings() => [AsString()];

    /// <inheritdoc />
    public override object AsObject() => value;
}
