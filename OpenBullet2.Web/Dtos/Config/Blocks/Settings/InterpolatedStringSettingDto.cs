namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents an interpolated string setting.
/// </summary>
public class InterpolatedStringSettingDto : InterpolatedSettingDto
{
    /// <summary>
    /// The string value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
