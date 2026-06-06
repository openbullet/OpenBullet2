namespace RuriLib.Models.Blocks.Settings.Interpolated;

/// <summary>
/// Represents an interpolated string block setting.
/// </summary>
public class InterpolatedStringSetting : InterpolatedSetting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public string? Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the string should be displayed as a multiline textbox.
    /// </summary>
    public bool MultiLine { get; set; } = false;
}
