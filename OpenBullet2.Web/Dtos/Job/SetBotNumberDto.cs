using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// DTO with information required to update the number of
/// bots in a job.
/// </summary>
public class SetBotNumberDto
{
    /// <summary>
    /// The id of the target job.
    /// </summary>
    [Required]
    public int JobId { get; set; }

    /// <summary>
    /// The number of bots to set.
    /// </summary>
    [Required]
    public int BotNumber { get; set; }
}
