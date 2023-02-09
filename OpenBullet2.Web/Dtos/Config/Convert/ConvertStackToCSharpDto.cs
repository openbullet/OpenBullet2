using RuriLib.Models.Blocks;
using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Convert;

/// <summary>
/// DTO used to convert a Stack of blocks to a C# script.
/// </summary>
public class ConvertStackToCSharpDto
{
    /// <summary>
    /// The id of the config (used to get the settings that
    /// influence the code generation process).
    /// </summary>
    [Required]
    public string ConfigId { get; set; } = string.Empty;

    /// <summary>
    /// The Stack of blocks to convert.
    /// </summary>
    [Required]
    public List<BlockInstance> Stack { get; set; } = new();
}
