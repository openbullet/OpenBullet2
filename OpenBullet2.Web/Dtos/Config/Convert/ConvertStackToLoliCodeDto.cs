using RuriLib.Models.Blocks;
using System.ComponentModel.DataAnnotations;

namespace OpenBullet2.Web.Dtos.Config.Convert;

/// <summary>
/// DTO used to convert a Stack of blocks to a LoliCode script.
/// </summary>
public class ConvertStackToLoliCodeDto
{
    /// <summary>
    /// The Stack of blocks to convert.
    /// </summary>
    [Required]
    public List<BlockInstance> Stack { get; set; } = new();
}
