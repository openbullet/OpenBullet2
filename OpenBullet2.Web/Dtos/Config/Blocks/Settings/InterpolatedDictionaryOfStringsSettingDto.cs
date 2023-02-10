using OpenBullet2.Web.Dtos.Config.Blocks.Settings;
/// <summary>
/// DTO that represents an interpolated dictionary of strings setting.
/// </summary>
public class InterpolatedDictionaryOfStringsSettingDto : InterpolatedSettingDto
{
    /// <summary>
    /// The dictionary of strings value.
    /// </summary>
    public Dictionary<string, string> Value { get; set; } = new();
}
