namespace RuriLib.Models.Conditions.Comparisons;

/// <summary>
/// Comparison operators for list variables.
/// </summary>
public enum ListComparison
{
    /// <summary>
    /// The list must contain the requested item.
    /// </summary>
    Contains,

    /// <summary>
    /// The list must not contain the requested item.
    /// </summary>
    DoesNotContain,

    /// <summary>
    /// The requested index or item must exist.
    /// </summary>
    Exists,

    /// <summary>
    /// The requested index or item must not exist.
    /// </summary>
    DoesNotExist
}
