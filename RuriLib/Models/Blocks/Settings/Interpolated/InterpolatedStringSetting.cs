namespace RuriLib.Models.Blocks.Settings.Interpolated;

/// <summary>
/// A setting that holds an interpolated string value.
/// </summary>
public class InterpolatedStringSetting : InterpolatedSetting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public string? Value { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the string should be displayed as a multiline textbox.
    /// </summary>
    public bool MultiLine { get; set; } = false;
}
