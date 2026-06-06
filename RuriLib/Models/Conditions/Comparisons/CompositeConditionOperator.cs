namespace RuriLib.Models.Conditions.Comparisons;

/// <summary>
/// Logical operators used to combine multiple conditions.
/// </summary>
public enum CompositeConditionOperator
{
    /// <summary>
    /// At least one condition must match.
    /// </summary>
    OR,

    /// <summary>
    /// All conditions must match.
    /// </summary>
    AND
}
