using RuriLib.Models.Blocks;

namespace OpenBullet2.Web.Dtos.Config.Convert;

/// <summary>
/// DTO that contains information about a converted Stack of blocks.
/// </summary>
public class ConvertedStackDto
{
    /// <summary>
    /// The Stack of blocks.
    /// </summary>
    public List<BlockInstance> Stack { get; set; } = new();
}
