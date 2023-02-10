using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents a multipart setting.
/// </summary>
public class HttpContentSettingsGroupDto
{
    /// <summary>
    /// The type of http content settings group.
    /// </summary>
    public HttpContentSettingsGroupType Type { get; set; }

    /// <summary>
    /// The name.
    /// </summary>
    public BlockSettingDto? Name { get; set; }

    /// <summary>
    /// The content type.
    /// </summary>
    public BlockSettingDto? ContentType { get; set; }
}

/// <summary>
/// The type of http content settings group.
/// </summary>
public enum HttpContentSettingsGroupType
{
    /// <summary>
    /// String http content settings group.
    /// </summary>
    String,

    /// <summary>
    /// Raw http content settings group.
    /// </summary>
    Raw,

    /// <summary>
    /// File http content settings group.
    /// </summary>
    File
}
