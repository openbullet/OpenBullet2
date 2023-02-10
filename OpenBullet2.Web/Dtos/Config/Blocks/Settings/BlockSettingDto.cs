using RuriLib.Models.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a block setting.
/// </summary>
public class BlockSettingDto
{
    /// <summary>
    /// The input mode of this setting.
    /// </summary>
    public SettingInputMode InputMode { get; set; }

    /// <summary>
    /// The input variable name, if the mode 
    /// is <see cref="SettingInputMode.Variable"/>.
    /// </summary>
    public string InputVariableName { get; set; } = string.Empty;

    /// <summary>
    /// The fixed setting, if the mode
    /// is <see cref="SettingInputMode.Fixed"/>.
    /// </summary>
    public object? FixedSetting { get; set; }

    /// <summary>
    /// The interpolated setting, if the mode
    /// is <see cref="SettingInputMode.Interpolated"/>.
    /// </summary>
    public object? InterpolatedSetting { get; set; }
}
