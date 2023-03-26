namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// An answer to a custom user input question.
/// </summary>
public class CustomInputAnswerDto
{
    /// <summary>
    /// The name of the variable that will be set, and that can be
    /// accessed via the input.VARNAME syntax.
    /// </summary>
    public string VariableName { get; set; } = string.Empty;

    /// <summary>
    /// The value to set.
    /// </summary>
    public string Answer { get; set; } = string.Empty;
}
