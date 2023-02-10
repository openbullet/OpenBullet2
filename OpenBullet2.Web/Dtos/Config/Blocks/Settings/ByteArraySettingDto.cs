namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed byte array setting.
/// </summary>
public class ByteArraySettingDto : SettingDto
{
    /// <summary>
    /// The byte array value.
    /// </summary>
    public byte[] Value { get; set; } = Array.Empty<byte>();
}
