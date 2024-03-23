using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.HttpRequest;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents standard request params.
/// </summary>
[PolyType("multipartRequestParams")]
[MapsFrom(typeof(MultipartRequestParams), false)]
[MapsTo(typeof(MultipartRequestParams), false)]
public class MultipartRequestParamsDto : RequestParamsDto
{
    /// <summary>
    /// The multipart contents.
    /// </summary>
    public List<object> Contents { get; set; } = new();

    /// <summary>
    /// The multipart boundary.
    /// </summary>
    public BlockSettingDto? Boundary { get; set; }
}
