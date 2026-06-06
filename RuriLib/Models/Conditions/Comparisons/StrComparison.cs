namespace RuriLib.Models.Conditions.Comparisons;

/// <summary>
/// Comparison operators for string values.
/// </summary>
public enum StrComparison
{
    /// <summary>
    /// The string must equal the comparison target.
    /// </summary>
    EqualTo,

    /// <summary>
    /// The string must differ from the comparison target.
    /// </summary>
    NotEqualTo,

    /// <summary>
    /// The string must contain the comparison target.
    /// </summary>
    Contains,

    /// <summary>
    /// The string must not contain the comparison target.
    /// </summary>
    DoesNotContain,

    /// <summary>
    /// The string must exist.
    /// </summary>
    Exists,

    /// <summary>
    /// The string must not exist.
    /// </summary>
    DoesNotExist,

    /// <summary>
    /// The string must match the regular expression.
    /// </summary>
    MatchesRegex,

    /// <summary>
    /// The string must not match the regular expression.
    /// </summary>
    DoesNotMatchRegex
}
