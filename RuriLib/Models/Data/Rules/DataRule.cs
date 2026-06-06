using System;

namespace RuriLib.Models.Data.Rules;

/// <summary>
/// Represents a rule evaluated against a sliced data value.
/// </summary>
public abstract class DataRule
{
    /// <summary>
    /// Whether the rule result should be inverted.
    /// </summary>
    public bool Invert { get; set; }

    /// <summary>
    /// The slice name the rule targets.
    /// </summary>
    public string SliceName { get; set; } = string.Empty;

    /// <summary>
    /// Determines whether the provided value satisfies the rule.
    /// </summary>
    /// <param name="value">The value to evaluate.</param>
    /// <returns><see langword="true"/> if the value satisfies the rule; otherwise <see langword="false"/>.</returns>
    public virtual bool IsSatisfied(string value)
        => throw new NotImplementedException();
}
