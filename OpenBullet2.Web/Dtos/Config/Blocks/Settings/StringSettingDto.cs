namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed string setting.
/// </summary>
public class StringSettingDto : SettingDto
{
    /// <summary>
    /// The string value.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
