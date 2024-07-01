namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// DTO that represents a block instance.
/// </summary>
public class BlockInstanceDto
{
    /// <summary>
    /// The id of the block's descriptor.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Whether the block is currently disabled.
    /// </summary>
    public bool Disabled { get; set; }

    /// <summary>
    /// The label assigned to the block.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The values of the settings of the block.
    /// </summary>
    public Dictionary<string, BlockSettingDto> Settings { get; set; } = new();

    /// <summary>
    /// The type of block instance.
    /// </summary>
    public BlockInstanceType Type { get; set; } = BlockInstanceType.Auto;
}
