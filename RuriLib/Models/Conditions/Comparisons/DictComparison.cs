namespace RuriLib.Models.Conditions.Comparisons;

/// <summary>
/// Comparison operators for dictionary variables.
/// </summary>
public enum DictComparison
{
    /// <summary>
    /// The dictionary must contain the specified key.
    /// </summary>
    HasKey,

    /// <summary>
    /// The dictionary must not contain the specified key.
    /// </summary>
    DoesNotHaveKey,

    /// <summary>
    /// The dictionary must contain the specified value.
    /// </summary>
    HasValue,

    /// <summary>
    /// The dictionary must not contain the specified value.
    /// </summary>
    DoesNotHaveValue,

    /// <summary>
    /// The requested entry must exist.
    /// </summary>
    Exists,

    /// <summary>
    /// The requested entry must not exist.
    /// </summary>
    DoesNotExist
}
