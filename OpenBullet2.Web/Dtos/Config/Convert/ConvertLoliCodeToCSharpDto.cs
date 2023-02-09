using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Convert;

/// <summary>
/// DTO used to convert a LoliCode script to a C# script.
/// </summary>
public class ConvertLoliCodeToCSharpDto
{
    /// <summary>
    /// The id of the config (used to get the settings that
    /// influence the code generation process).
    /// </summary>
    [Required]
    public string ConfigId { get; set; } = string.Empty;

    /// <summary>
    /// The LoliCode script to convert.
    /// </summary>
    [Required]
    public string LoliCode { get; set; } = string.Empty;
}
