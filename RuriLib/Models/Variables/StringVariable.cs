using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents a string variable.
/// </summary>
public class StringVariable : Variable
{
    private readonly string value;

    /// <summary>
    /// Creates a string variable.
    /// </summary>
    /// <param name="value">The string value.</param>
    public StringVariable(string value)
    {
        this.value = value ?? throw new ArgumentNullException(nameof(value));
        Type = VariableType.String;
    }

    /// <inheritdoc />
    public override string AsString() => value;

    /// <inheritdoc />
    public override int AsInt()
    {
        if (int.TryParse(value, out var result))
        {
            return result;
        }

        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public override long AsLong()
    {
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public override bool AsBool()
    {
        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public override byte[] AsByteArray() => Encoding.UTF8.GetBytes(value);

    /// <inheritdoc />
    public override float AsFloat()
    {
        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public override double AsDouble()
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new InvalidCastException();
    }

    /// <inheritdoc />
    public override List<string> AsListOfStrings() => [value];

    /// <inheritdoc />
    public override object AsObject() => value;
}
