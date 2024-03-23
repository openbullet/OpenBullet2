using RuriLib.Models.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// DTO that represents a block setting.
/// </summary>
public class BlockSettingDto
{
    /// <summary>
    /// The setting's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The value of the setting. Depends on the input mode
    /// and the type of setting.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The input variable name, only used if the input mode
    /// is <see cref="SettingInputMode.Variable" />.
    /// </summary>
    public string? InputVariableName { get; set; }

    /// <summary>
    /// The input mode of this setting.
    /// </summary>
    public SettingInputMode InputMode { get; set; }

    /// <summary>
    /// The type of block setting.
    /// </summary>
    public BlockSettingType Type { get; set; } = BlockSettingType.None;
}
