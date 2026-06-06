using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Variables;

/// <summary>
/// Represents a dictionary-of-strings variable.
/// </summary>
public class DictionaryOfStringsVariable : Variable
{
    private readonly Dictionary<string, string>? value;

    /// <summary>
    /// Creates a dictionary-of-strings variable.
    /// </summary>
    /// <param name="value">The dictionary value.</param>
    public DictionaryOfStringsVariable(Dictionary<string, string>? value)
    {
        this.value = value;
        Type = VariableType.DictionaryOfStrings;
    }

    /// <inheritdoc />
    public override string AsString() => value is null
        ? "null"
        : "{" + string.Join(", ", AsListOfStrings()!.Select(s => $"({s})")) + "}";

    /// <inheritdoc />
    public override List<string>? AsListOfStrings() =>
        value?.Select(kvp => $"{kvp.Key}, {kvp.Value}").ToList();

    /// <inheritdoc />
    public override Dictionary<string, string>? AsDictionaryOfStrings()
        => value;

    /// <inheritdoc />
    public override object? AsObject() => value;
}
