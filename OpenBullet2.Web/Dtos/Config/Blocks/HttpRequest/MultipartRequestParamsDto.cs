using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents standard request params.
/// </summary>
public class MultipartRequestParamsDto : RequestParamsDto
{
    /// <summary></summary>
    public MultipartRequestParamsDto()
    {
        Type = RequestParamsType.Multipart;
    }

    /// <summary>
    /// The multipart contents.
    /// </summary>
    public List<object> Contents { get; set; } = new();

    /// <summary>
    /// The multipart boundary.
    /// </summary>
    public BlockSettingDto? Boundary { get; set; }
}
