namespace OpenBullet2.Web.Dtos.Config.Blocks.HttpRequest;

/// <summary>
/// DTO that represents HTTP request params.
/// </summary>
public class RequestParamsDto
{
    /// <summary>
    /// The type of request params.
    /// </summary>
    public RequestParamsType Type { get; set; }
}

/// <summary>
/// The type of request params.
/// </summary>
public enum RequestParamsType
{
    /// <summary>
    /// Standard request params.
    /// </summary>
    Standard,

    /// <summary>
    /// Raw request params.
    /// </summary>
    Raw,

    /// <summary>
    /// Basic auth request params.
    /// </summary>
    BasicAuth,

    /// <summary>
    /// Multipart request params.
    /// </summary>
    Multipart
}