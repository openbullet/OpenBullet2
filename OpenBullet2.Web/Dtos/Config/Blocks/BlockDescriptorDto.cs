using RuriLib.Models.Variables;

namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// DTO that contains information about a block.
/// </summary>
public class BlockDescriptorDto
{
    /// <summary>
    /// The unique id of the block.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The original name of the block.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The description of the block.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The extra information about the block.
    /// </summary>
    public string ExtraInfo { get; set; } = string.Empty;

    /// <summary>
    /// The type of value returned by the block, if any.
    /// </summary>
    public VariableType? ReturnType { get; set; } = null;

    /// <summary>
    /// The block's category.
    /// </summary>
    public BlockCategoryDto? Category { get; set; } = null;

    /// <summary>
    /// The block's parameters.
    /// </summary>
    public Dictionary<string, object?> Parameters { get; set; } = new();
}
