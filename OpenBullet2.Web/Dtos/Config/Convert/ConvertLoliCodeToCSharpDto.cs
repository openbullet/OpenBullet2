using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Convert;

/// <summary>
/// DTO used to convert a LoliCode script to a C# script.
/// </summary>
public class ConvertLoliCodeToCSharpDto
{
    /// <summary>
    /// The config settings, required during the code generation process.
    /// </summary>
    [Required]
    public ConfigSettingsDto Settings { get; set; } = default!;

    /// <summary>
    /// The LoliCode script to convert.
    /// </summary>
    [Required]
    public string LoliCode { get; set; } = string.Empty;
}
