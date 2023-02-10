using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents raw request params.
/// </summary>
public class RawRequestParamsDto : RequestParamsDto
{
    /// <summary></summary>
    public RawRequestParamsDto()
    {
        Type = RequestParamsType.Raw;
    }

    /// <summary>
    /// The request content.
    /// </summary>
    public BlockSettingDto? Content { get; set; }

    /// <summary>
    /// The content type.
    /// </summary>
    public BlockSettingDto? ContentType { get; set; }
}
