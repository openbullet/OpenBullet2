namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed integer setting.
/// </summary>
public class IntSettingDto : SettingDto
{
    /// <summary>
    /// The integer value.
    /// </summary>
    public int Value { get; set; } = 0;
}
