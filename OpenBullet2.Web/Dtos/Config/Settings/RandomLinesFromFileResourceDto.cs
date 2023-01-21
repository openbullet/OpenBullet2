using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Settings;

/// <summary>
/// DTO that contains the options of a resource which reads
/// random lines from a file.
/// </summary>
public class RandomLinesFromFileResourceDto
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
    /// Whether to skip empty lines when taking lines from the file.
    /// </summary>
    public bool IgnoreEmptyLines { get; set; } = true;

    /// <summary>
    /// True to only take lines that haven't been taken yet, false
    /// to take completely random lines each time (never runs out of lines).
    /// </summary>
    public bool Unique { get; set; } = false;
}
