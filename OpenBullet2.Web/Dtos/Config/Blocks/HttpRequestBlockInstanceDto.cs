namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// DTO that represents an http request block instance.
/// </summary>
public class HttpRequestBlockInstanceDto : BlockInstanceDto
{
    /// <summary>
    /// The request params.
    /// </summary>
    public object? RequestParams { get; set; }

    /// <summary>
    /// Whether any error in the block should be safely caught, without
    /// interrupting the execution.
    /// </summary>
    public bool Safe { get; set; }
}
