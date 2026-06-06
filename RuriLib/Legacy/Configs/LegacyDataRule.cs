namespace RuriLib.Legacy.Configs;

/// <summary>
/// Represents a legacy data rule entry.
/// </summary>
public class LegacyDataRule
{
    /// <summary>
    /// The slice name the rule applies to.
    /// </summary>
    public string SliceName { get; set; } = string.Empty;

    /// <summary>
    /// The legacy rule type.
    /// </summary>
    public LegacyRuleType RuleType { get; set; }

    /// <summary>
    /// The value associated with the rule.
    /// </summary>
    public string RuleString { get; set; } = string.Empty; // Options: "Lowercase", "Uppercase", "Digit", "Symbol" or customized
}

/// <summary>
/// Enumerates the rule types supported by legacy configs.
/// </summary>
public enum LegacyRuleType
{
    /// <summary>The slice must contain the given characters.</summary>
    MustContain,

    /// <summary>The slice must not contain the given characters.</summary>
    MustNotContain,

    /// <summary>The slice's length must be greater or equal to a given number.</summary>
    MinLength,

    /// <summary>The slice's length must be smaller or equal to a given number.</summary>
    MaxLength,

    /// <summary>The slice must match a given regex pattern.</summary>
    MustMatchRegex
}
