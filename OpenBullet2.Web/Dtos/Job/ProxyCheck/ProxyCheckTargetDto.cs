using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Job.ProxyCheck;

/// <summary>
/// A proxy check target site.
/// </summary>
public class ProxyCheckTargetDto
{
    /// <summary>
    /// The URL of the website that the proxy will send a GET query to.
    /// </summary>
    [Required]
    public string Url { get; set; } = "https://google.com";

    /// <summary>
    /// A keyword that must be present in the HTTP response body in order
    /// to mark the proxy as working. Case sensitive.
    /// </summary>
    [Required]
    public string SuccessKey { get; set; } = "title>Google";
}
