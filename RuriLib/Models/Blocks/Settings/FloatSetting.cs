namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// Represents a floating-point block setting.
/// </summary>
public class FloatSetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Whether generated code should pass this setting as a <see cref="double"/> instead of a <see cref="float"/>.
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public bool UseDouble { get; set; } = true;
}
