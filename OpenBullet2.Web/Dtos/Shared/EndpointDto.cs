using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Shared;

/// <summary>
/// DTO that represents a shared endpoint.
/// </summary>
public class EndpointDto
{
    /// <summary>
    /// The route of this endpoint in the URI.
    /// </summary>
    [Required, MinLength(1), RegularExpression(@"^[\w-]+$")]
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// The valid API keys that can be used to access this endpoint.
    /// </summary>
    [Required]
    public IEnumerable<string> ApiKeys { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The IDs of the configs that this endpoint should expose.
    /// </summary>
    [Required]
    public IEnumerable<string> ConfigIds { get; set; } = Array.Empty<string>();
}
