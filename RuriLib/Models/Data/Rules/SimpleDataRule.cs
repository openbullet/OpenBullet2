using System;
using System.Linq;

namespace RuriLib.Models.Data.Rules;

/// <summary>
/// Evaluates a string against a simple comparison rule.
/// </summary>
public class SimpleDataRule : DataRule
{
    /// <summary>
    /// The comparison to perform.
    /// </summary>
    public StringRule Comparison { get; set; } = StringRule.EqualTo;

    /// <summary>
    /// The comparison operand.
    /// </summary>
    public string StringToCompare { get; set; } = string.Empty;

    /// <summary>
    /// Whether string comparisons should be case-sensitive.
    /// </summary>
    public bool CaseSensitive { get; set; } = true;

    /// <inheritdoc/>
    public override bool IsSatisfied(string value)
    {
        var cs = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        return Invert ^ Comparison switch
        {
            StringRule.EqualTo => value.Equals(StringToCompare, cs),
            StringRule.Contains => value.Contains(StringToCompare, cs),
            StringRule.LongerThan => value.Length > int.Parse(StringToCompare),
            StringRule.ShorterThan => value.Length < int.Parse(StringToCompare),
            StringRule.ContainsAll => StringToCompare.All(c => value.IndexOf(c, cs) != -1),
            StringRule.ContainsAny => StringToCompare.Any(c => value.IndexOf(c, cs) != -1),
            StringRule.StartsWith => value.StartsWith(StringToCompare, cs),
            StringRule.EndsWith => value.EndsWith(StringToCompare, cs),
            _ => throw new NotImplementedException()
        };
    }
}

/// <summary>
/// Enumerates the simple string comparisons supported by <see cref="SimpleDataRule"/>.
/// </summary>
public enum StringRule
{
    /// <summary>
    /// Checks whether the value equals the comparison string.
    /// </summary>
    EqualTo,

    /// <summary>
    /// Checks whether the value contains the comparison string.
    /// </summary>
    Contains,

    /// <summary>
    /// Checks whether the value length is greater than the comparison number.
    /// </summary>
    LongerThan,

    /// <summary>
    /// Checks whether the value length is smaller than the comparison number.
    /// </summary>
    ShorterThan,

    /// <summary>
    /// Checks whether the value contains all characters from the comparison string.
    /// </summary>
    ContainsAll,

    /// <summary>
    /// Checks whether the value contains any character from the comparison string.
    /// </summary>
    ContainsAny,

    /// <summary>
    /// Checks whether the value starts with the comparison string.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Checks whether the value ends with the comparison string.
    /// </summary>
    EndsWith
}
