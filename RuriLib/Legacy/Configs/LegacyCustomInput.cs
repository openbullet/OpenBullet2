namespace RuriLib.Legacy.Configs;

/// <summary>
/// Describes a legacy custom input.
/// </summary>
public class LegacyCustomInput
{
    /// <summary>
    /// The target variable name.
    /// </summary>
    public string VariableName { get; set; } = string.Empty;

    /// <summary>
    /// The input description shown to the user.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
