using OpenBullet2.Web.Attributes;
using RuriLib.Models.Blocks.Custom.HttpRequest;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents basic auth request params.
/// </summary>
[PolyType("basicAuthRequestParams")]
[MapsFrom(typeof(BasicAuthRequestParams))]
[MapsTo(typeof(BasicAuthRequestParams), false)]
public class BasicAuthRequestParamsDto : RequestParamsDto
{
    /// <summary>
    /// The username.
    /// </summary>
    public BlockSettingDto? Username { get; set; }

    /// <summary>
    /// The password.
    /// </summary>
    public BlockSettingDto? Password { get; set; }
}
