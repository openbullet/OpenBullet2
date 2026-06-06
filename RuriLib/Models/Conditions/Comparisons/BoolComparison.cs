namespace RuriLib.Models.Conditions.Comparisons;

/// <summary>
/// Comparison operators for boolean values.
/// </summary>
public enum BoolComparison
{
    /// <summary>
    /// The value must match the expected boolean value.
    /// </summary>
    Is,

    /// <summary>
    /// The value must differ from the expected boolean value.
    /// </summary>
    IsNot
}
