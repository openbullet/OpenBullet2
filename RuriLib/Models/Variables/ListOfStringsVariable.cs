using System.Collections.Generic;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents a list-of-strings variable.
/// </summary>
public class ListOfStringsVariable : Variable
{
    private readonly List<string>? value;

    /// <summary>
    /// Creates a list-of-strings variable.
    /// </summary>
    /// <param name="value">The list value.</param>
    public ListOfStringsVariable(List<string>? value)
    {
        this.value = value;
        Type = VariableType.ListOfStrings;
    }

    /// <inheritdoc />
    public override string AsString() => value is null
        ? "null"
        : "[" + string.Join(", ", value) + "]";

    /// <inheritdoc />
    public override List<string>? AsListOfStrings() => value;

    /// <inheritdoc />
    public override object? AsObject() => value;
}
