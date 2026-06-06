namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// Represents an integer block setting.
/// </summary>
public class IntSetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public long Value { get; set; }

    /// <summary>
    /// Whether generated code should pass this setting as a <see cref="long"/> instead of an <see cref="int"/>.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public bool UseLong { get; set; } = true;
}
