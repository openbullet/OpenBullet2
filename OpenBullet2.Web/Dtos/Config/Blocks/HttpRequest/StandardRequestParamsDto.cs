using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents standard request params.
/// </summary>
public class StandardRequestParamsDto : RequestParamsDto
{
    /// <summary></summary>
    public StandardRequestParamsDto()
    {
        Type = RequestParamsType.Standard;
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
