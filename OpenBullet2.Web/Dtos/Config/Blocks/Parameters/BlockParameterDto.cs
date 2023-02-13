using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Parameters;

/// <summary>
/// DTO that contains information about a parameter of a block.
/// </summary>
public class BlockParameterDto : PolyDto
{
    /// <summary>
    /// The parameter's unique auto-generated name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The name assigned by the coder.
    /// </summary>
    public string AssignedName { get; set; } = string.Empty;

    /// <summary>
    /// The pretty name to display.
    /// </summary>
    public string PrettyName => AssignedName ?? Name.ToReadableName();

    /// <summary>
    /// The description of the parameter.
    /// </summary>
    public string? Description { get; set; } = null;

    /// <summary>
    /// The default input mode.
    /// </summary>
    public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;

    /// <summary>
    /// The default variable name.
    /// </summary>
    public string DefaultVariableName { get; set; } = string.Empty;
}
