using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Hit;

/// <summary>
/// DTO that contains information about some fields of a hit
/// that can be updated.
/// </summary>
public class UpdateHitDto
{
    /// <summary>
    /// The id of the hit to update.
    /// </summary>
    [Required]
    public int Id { get; set; }

    /// <summary>
    /// The data that was provided to the bot to get the hit.
    /// </summary>
    [Required]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// The variables captured by the bot.
    /// </summary>
    [Required]
    public string CapturedData { get; set; } = string.Empty;

    /// <summary>
    /// The type of hit, for example SUCCESS, NONE, CUSTOM etc.
    /// </summary>
    [Required]
    public string Type { get; set; } = string.Empty;
}
