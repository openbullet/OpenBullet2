namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Custom user inputs.
/// </summary>
public class CustomInputsDto
{
    /// <summary>
    /// The job id for which to set inputs.
    /// </summary>
    public required int JobId { get; set; }

    /// <summary>
    /// The custom inputs values.
    /// </summary>
    public required IEnumerable<CustomInputAnswerDto> Answers { get; set; }
}
