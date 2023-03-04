using RuriLib.Models.Jobs;
using System.Text.Json.Serialization;

namespace OpenBullet2.Web.Dtos.Job;

/// <summary>
/// Base overview info about a job.
/// </summary>
public class JobOverviewDto
{
    /// <summary>
    /// The job id.
    /// </summary>
    [JsonPropertyOrder(-3)]
    public int Id { get; set; }

    /// <summary>
    /// The id of the owner of this job.
    /// </summary>
    [JsonPropertyOrder(-2)]
    public int OwnerId { get; set; }

    /// <summary>
    /// The job status.
    /// </summary>
    [JsonPropertyOrder(-1)]
    public JobStatus Status { get; set; }
}
