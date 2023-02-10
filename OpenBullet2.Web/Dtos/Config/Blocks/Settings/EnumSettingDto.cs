namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed enumeration setting.
/// </summary>
public class EnumSettingDto : SettingDto
{
    /// <summary>
    /// The enumeration value as a string.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
