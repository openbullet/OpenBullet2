namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// Represents a byte-array block setting.
/// </summary>
public class ByteArraySetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public byte[]? Value { get; set; } = [];
}
