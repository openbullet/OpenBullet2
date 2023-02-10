namespace OpenBullet2.Web.Dtos.Config.Blocks.Settings;

/// <summary>
/// DTO that represents a fixed floating point number setting.
/// </summary>
public class FloatSettingDto : SettingDto
{
    /// <summary>
    /// The floating point number value.
    /// </summary>
    public float Value { get; set; } = 0f;
}
