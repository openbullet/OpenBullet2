namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Custom user inputs.
/// </summary>
public class CustomInputsDto
{
    /// <summary>
    /// The job id for which to set inputs.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The custom inputs values.
    /// </summary>
    public IEnumerable<CustomInputAnswerDto> Inputs { get; set; }
}
