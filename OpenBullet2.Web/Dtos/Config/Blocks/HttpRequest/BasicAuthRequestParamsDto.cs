using OpenBullet2.Web.Dtos.Config.Blocks.Settings;

namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents basic auth request params.
/// </summary>
public class BasicAuthRequestParamsDto : RequestParamsDto
{
    /// <summary></summary>
    public BasicAuthRequestParamsDto()
    {
        Type = RequestParamsType.BasicAuth;
    }

    /// <summary>
    /// The username.
    /// </summary>
    public BlockSettingDto? Username { get; set; }

    /// <summary>
    /// The password.
    /// </summary>
    public BlockSettingDto? Password { get; set; }
}
