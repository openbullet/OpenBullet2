namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// A setting that holds a string value.
/// </summary>
public class StringSetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public string? Value { get; set; }
    
    /// <summary>
    /// Whether the string should be displayed as a multiline textbox.
    /// </summary>
    public bool MultiLine { get; set; } = false;
}
