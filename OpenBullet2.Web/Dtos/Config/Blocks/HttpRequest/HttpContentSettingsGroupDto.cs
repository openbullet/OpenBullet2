namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a multipart setting.
/// </summary>
public class HttpContentSettingsGroupDto : PolyDto
{
    /// <summary>
    /// The name.
    /// </summary>
    public BlockSettingDto? Name { get; set; }

    /// <summary>
    /// The content type.
    /// </summary>
    public BlockSettingDto? ContentType { get; set; }
}
