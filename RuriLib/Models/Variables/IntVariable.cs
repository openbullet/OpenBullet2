using System;
using System.Collections.Generic;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents an integer variable.
/// </summary>
public class IntVariable : Variable
{
    private readonly long value;

    /// <summary>
    /// Creates an integer variable.
    /// </summary>
    /// <param name="value">The integer value.</param>
    public IntVariable(long value)
    {
        this.value = value;
        Type = VariableType.Int;
    }

    /// <inheritdoc />
    public override string AsString() => value.ToString();

    /// <inheritdoc />
    public override int AsInt() => Convert.ToInt32(value);

    /// <inheritdoc />
    public override long AsLong() => value;

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
