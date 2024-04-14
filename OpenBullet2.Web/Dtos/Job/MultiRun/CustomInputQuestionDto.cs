namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// A question for a custom user input.
/// </summary>
public class CustomInputQuestionDto
{
    /// <summary>
    /// The description of what this custom input controls.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The name of the variable that will be set, and that can be
    /// accessed via the input.VARNAME syntax.
    /// </summary>
    public string VariableName { get; set; } = string.Empty;

    /// <summary>
    /// The default answer to suggest to the user.
    /// </summary>
    public string DefaultAnswer { get; set; } = string.Empty;
    
    /// <summary>
    /// The current answer to the question, null if not answered yet.
    /// </summary>
    public string? CurrentAnswer { get; set; } = null;
}
