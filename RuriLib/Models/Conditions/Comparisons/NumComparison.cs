namespace RuriLib.Models.Conditions.Comparisons;

/// <summary>
/// Comparison operators for numeric values.
/// </summary>
public enum NumComparison
{
    /// <summary>
    /// The value must equal the comparison target.
    /// </summary>
    EqualTo,

    /// <summary>
    /// The value must differ from the comparison target.
    /// </summary>
    NotEqualTo,

    /// <summary>
    /// The value must be lower than the comparison target.
    /// </summary>
    LessThan,

    /// <summary>
    /// The value must be lower than or equal to the comparison target.
    /// </summary>
    LessThanOrEqualTo,

    /// <summary>
    /// The value must be greater than the comparison target.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// The value must be greater than or equal to the comparison target.
    /// </summary>
    GreaterThanOrEqualTo
}
