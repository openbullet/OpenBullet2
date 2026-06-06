namespace RuriLib.Legacy.Functions.Conditions;

/// <summary>
/// Represents a condition of a keycheck.
/// </summary>
public struct KeycheckCondition
{
    /// <summary>
    /// Initializes a keycheck condition.
    /// </summary>
    /// <param name="left">The left term.</param>
    /// <param name="comparer">The comparison operator.</param>
    /// <param name="right">The right term.</param>
    public KeycheckCondition(string left, Comparer comparer, string right)
    {
        Left = left;
        Comparer = comparer;
        Right = right;
    }

    /// <summary>
    /// The left term.
    /// </summary>
    public string Left;

    /// <summary>
    /// The comparison operator.
    /// </summary>
    public Comparer Comparer;

    /// <summary>
    /// The right term.
    /// </summary>
    public string Right;
}
