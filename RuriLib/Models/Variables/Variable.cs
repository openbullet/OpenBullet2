using System;
using System.Collections.Generic;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents a typed runtime variable value.
/// </summary>
public abstract class Variable
{
    /// <summary>
    /// Gets or sets the variable name.
    /// </summary>
    public string Name { get; set; } = "variable";

    /// <summary>
    /// Gets or sets a value indicating whether the variable should be captured.
    /// </summary>
    public bool MarkedForCapture { get; set; }

    /// <summary>
    /// Gets or sets the variable type.
    /// </summary>
    public VariableType Type { get; set; } = VariableType.String;

    /// <summary>
    /// Converts the variable to a string representation.
    /// </summary>
    /// <returns>The string representation.</returns>
    public virtual string AsString() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to an integer.
    /// </summary>
    /// <returns>The integer representation.</returns>
    public virtual int AsInt() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to a long integer.
    /// </summary>
    /// <returns>The long integer representation.</returns>
    public virtual long AsLong() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to a floating-point number.
    /// </summary>
    /// <returns>The floating-point representation.</returns>
    public virtual float AsFloat() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to a double-precision floating-point number.
    /// </summary>
    /// <returns>The double-precision floating-point representation.</returns>
    public virtual double AsDouble() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to a boolean.
    /// </summary>
    /// <returns>The boolean representation.</returns>
    public virtual bool AsBool() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to a list of strings.
    /// </summary>
    /// <returns>The list representation.</returns>
    public virtual List<string>? AsListOfStrings() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to a dictionary of strings.
    /// </summary>
    /// <returns>The dictionary representation.</returns>
    public virtual Dictionary<string, string>? AsDictionaryOfStrings() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to a byte array.
    /// </summary>
    /// <returns>The byte-array representation.</returns>
    public virtual byte[]? AsByteArray() => throw new InvalidCastException();

    /// <summary>
    /// Converts the variable to its raw object value.
    /// </summary>
    /// <returns>The underlying object representation.</returns>
    public virtual object? AsObject() => throw new InvalidCastException();
}
