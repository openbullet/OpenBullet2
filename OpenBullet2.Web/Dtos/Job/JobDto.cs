using OpenBullet2.Core.Models.Jobs;
using RuriLib.Models.Jobs;
using System.Text.Json.Serialization;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Base information about a job.
/// </summary>
public class JobDto
{
    /// <summary>
    /// The job id.
    /// </summary>
    [JsonPropertyOrder(-6)]
    public int Id { get; set; }

    /// <summary>
    /// The id of the owner of this job.
    /// </summary>
    [JsonPropertyOrder(-5)]
    public int OwnerId { get; set; }

    /// <summary>
    /// The job type.
    /// </summary>
    [JsonPropertyOrder(-4)]
    public JobType Type { get; set; }

    /// <summary>
    /// The job status.
    /// </summary>
    [JsonPropertyOrder(-3)]
    public JobStatus Status { get; set; }

    /// <summary>
    /// The name of the job.
    /// </summary>
    [JsonPropertyOrder(-2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// When the job was started, if it was started.
    /// </summary>
    [JsonPropertyOrder(-1)]
    public DateTime? StartTime { get; set; }
}
