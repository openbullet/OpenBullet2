using System;
using System.Collections.Generic;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents a boolean variable.
/// </summary>
public class BoolVariable : Variable
{
    private readonly bool value;

    /// <summary>
    /// Creates a boolean variable.
    /// </summary>
    /// <param name="value">The boolean value.</param>
    public BoolVariable(bool value)
    {
        this.value = value;
        Type = VariableType.Bool;
    }

    /// <inheritdoc />
    public override string AsString() => value.ToString();

    /// <inheritdoc />
    public override int AsInt() => value ? 1 : 0;

    /// <inheritdoc />
    public override long AsLong() => value ? 1 : 0;

    /// <inheritdoc />
    public override bool AsBool() => value;

    /// <inheritdoc />
    public override byte[] AsByteArray() => BitConverter.GetBytes(value);

    /// <inheritdoc />
    public override float AsFloat() => value ? 1 : 0;

    /// <inheritdoc />
    public override double AsDouble() => value ? 1 : 0;

    /// <inheritdoc />
    public override List<string> AsListOfStrings() => [AsString()];

    /// <inheritdoc />
    public override object AsObject() => value;
}
