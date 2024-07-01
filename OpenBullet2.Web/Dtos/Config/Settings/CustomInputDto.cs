namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// A custom input that can be provided to the config at runtime.
/// </summary>
public class CustomInputDto
{
    /// <summary>
    /// The description of the custom input.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The name of the variable that will contain the custom value.
    /// </summary>
    public string VariableName { get; set; } = string.Empty;

    /// <summary>
    /// The default value.
    /// </summary>
    public string DefaultAnswer { get; set; } = string.Empty;
}
