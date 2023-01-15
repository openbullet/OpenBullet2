using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO used to create a wordlist that references
/// an existing file on disk.
/// </summary>
public class CreateWordlistDto
{
    /// <summary>
    /// The name of the wordlist.
    /// </summary>
    [Required]
    public string Name { get; set; } = default!;

    /// <summary>
    /// The purpose of the wordlist.
    /// </summary>
    public string Purpose { get; set; } = default!;

    /// <summary>
    /// The wordlist type.
    /// </summary>
    public string WordlistType { get; set; } = "Default";

    /// <summary>
    /// The path to the actual file on disk.
    /// </summary>
    [Required]
    public string FilePath { get; set; } = default!;
}
