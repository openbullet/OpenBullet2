namespace RuriLib.Models.Blocks;

/// <summary>
/// Describes an image attached to a block.
/// </summary>
public class BlockImageInfo
{
    /// <summary>
    /// The display name of the image.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The maximum width of the rendered image.
    /// </summary>
    public int MaxWidth { get; set; }

    /// <summary>
    /// The maximum height of the rendered image.
    /// </summary>
    public int MaxHeight { get; set; }

    /// <summary>
    /// The image bytes.
    /// </summary>
    public byte[] Value { get; set; } = [];
}
