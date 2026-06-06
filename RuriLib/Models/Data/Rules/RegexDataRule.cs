using System.Text.RegularExpressions;

namespace RuriLib.Models.Data.Rules;

/// <summary>
/// Evaluates a value against a regular expression.
/// </summary>
public class RegexDataRule : DataRule
{
    /// <summary>
    /// The regular expression to match.
    /// </summary>
    public string RegexToMatch { get; set; } = "^.*$";

    /// <inheritdoc/>
    public override bool IsSatisfied(string value)
        => Invert ^ Regex.IsMatch(value, RegexToMatch);
}
