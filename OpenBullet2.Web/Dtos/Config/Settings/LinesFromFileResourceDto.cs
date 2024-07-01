using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains the options of a resource which reads
/// lines from a file.
/// </summary>
public class LinesFromFileResourceDto
{
    /// <summary>
    /// The unique name of the resource.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The location of the file on disk.
    /// </summary>
    [Required]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Whether to go back to the start of the file if there are
    /// no more lines to read.
    /// </summary>
    public bool LoopsAround { get; set; } = true;

    /// <summary>
    /// Whether to skip empty lines when taking lines from the file.
    /// </summary>
    public bool IgnoreEmptyLines { get; set; } = true;
}
