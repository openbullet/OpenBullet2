using RuriLib.Models.Variables;

namespace OpenBullet2.Web.Dtos.ConfigDebugger;

/// <summary>
/// DTO that contains information about a variable set while running
/// a config.
/// </summary>
public class VariableDto
{
    /// <summary>
    /// The name of the variable.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether the variable is marked for capture.
    /// </summary>
    public bool MarkedForCapture { get; set; } = false;

    /// <summary>
    /// The type of variable.
    /// </summary>
    public VariableType Type { get; set; } = VariableType.String;

    /// <summary>
    /// The value of the variable.
    /// </summary>
    public object? Value { get; set; } = null;
}
