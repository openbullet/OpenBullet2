namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains the options of the configured resources.
/// </summary>
public class ResourcesDto
{
    /// <summary>
    /// The list of resources that take lines from a file.
    /// </summary>
    public List<LinesFromFileResourceDto> LinesFromFile { get; set; } = new();

    /// <summary>
    /// The list of resources that take random lines from a file.
    /// </summary>
    public List<RandomLinesFromFileResourceDto> RandomLinesFromFile { get; set; } = new();
}
