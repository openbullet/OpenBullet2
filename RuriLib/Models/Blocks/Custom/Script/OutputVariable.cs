using RuriLib.Models.Variables;

namespace RuriLib.Models.Blocks.Custom.Script;

/// <summary>
/// Declares an output variable exposed by a custom script block.
/// </summary>
public class OutputVariable
{
    /// <summary>
    /// Gets or sets the variable type.
    /// </summary>
    public VariableType Type { get; set; } = VariableType.String;
    /// <summary>
    /// Gets or sets the variable name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
