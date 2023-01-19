using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Wordlist;

/// <summary>
/// DTO to update a wordlist's info.
/// </summary>
public class UpdateWordlistInfoDto
{
    /// <summary>
    /// The id of the wordlist to update.
    /// </summary>
    [Required]
    public int Id { get; set; }

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
}
