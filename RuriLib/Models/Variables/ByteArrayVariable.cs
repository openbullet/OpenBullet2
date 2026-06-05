using System;
using System.Collections.Generic;
using System.Text;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents a byte-array variable.
/// </summary>
public class ByteArrayVariable : Variable
{
    private readonly byte[]? value;

    /// <summary>
    /// Creates a byte-array variable.
    /// </summary>
    /// <param name="value">The byte-array value.</param>
    public ByteArrayVariable(byte[]? value)
    {
        this.value = value;
        Type = VariableType.ByteArray;
    }

    /// <inheritdoc />
    public override string AsString() => value is null ? "null" : Encoding.UTF8.GetString(value);

    /// <inheritdoc />
    public override int AsInt() => BitConverter.ToInt32(value ?? throw new InvalidCastException(), 0);

    /// <inheritdoc />
    public override long AsLong() => BitConverter.ToInt64(value ?? throw new InvalidCastException(), 0);

    /// <inheritdoc />
    public override bool AsBool() => BitConverter.ToBoolean(value ?? throw new InvalidCastException(), 0);

    /// <inheritdoc />
    public override byte[]? AsByteArray() => value;

    /// <inheritdoc />
    public override float AsFloat() => AsInt();

    /// <inheritdoc />
    public override double AsDouble() => BitConverter.ToDouble(value ?? throw new InvalidCastException(), 0);

    /// <inheritdoc />
    public override List<string> AsListOfStrings() => [AsString()];

    /// <inheritdoc />
    public override object? AsObject() => value;
}
