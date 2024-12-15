using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings.Interpolated;

namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// A setting of a block.
/// </summary>
public class BlockSetting
{
    /// <summary>
    /// The input mode of the setting.
    /// </summary>
    public SettingInputMode InputMode { get; set; } = SettingInputMode.Fixed;

    /// <summary>
    /// The name of the setting.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    private string? _readableName;
    
    /// <summary>
    /// The readable name of the setting.
    /// </summary>
    public string ReadableName
    {
        // Failsafe in case ReadableName is never set
        get => _readableName ?? Name.ToReadableName();
        set => _readableName = value;
    }
    
    /// <summary>
    /// The description of the setting.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// The input variable name of the setting, in case it's in variable mode.
    /// </summary>
    public string InputVariableName { get; set; } = string.Empty;

    /// <summary>
    /// The fixed setting, in case it's in fixed mode.
    /// </summary>
    public Setting? FixedSetting { get; set; }
    
    /// <summary>
    /// The interpolated setting, in case it's in interpolated mode.
    /// </summary>
    public InterpolatedSetting? InterpolatedSetting { get; set; }
}
