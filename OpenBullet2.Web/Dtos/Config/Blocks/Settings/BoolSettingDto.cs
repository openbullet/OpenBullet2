namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed boolean setting.
/// </summary>
public class BoolSettingDto : SettingDto
{
    /// <summary>
    /// The boolean value.
    /// </summary>
    public bool Value { get; set; } = false;
}
