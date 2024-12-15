namespace RuriLib.Models.Blocks.Settings;

/// <summary>
/// A setting that holds a byte array value.
/// </summary>
public class ByteArraySetting : Setting
{
    /// <summary>
    /// The value of the setting.
    /// </summary>
    public byte[]? Value { get; set; }
}
