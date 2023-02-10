using OpenBullet2.Web.Dtos.Config.Blocks.Settings;
/// <summary>
/// DTO that represents an interpolated list of strings setting.
/// </summary>
public class InterpolatedListOfStringsSettingDto : InterpolatedSettingDto
{
    /// <summary>
    /// The list of strings value.
    /// </summary>
    public List<string> Value { get; set; } = new();
}
