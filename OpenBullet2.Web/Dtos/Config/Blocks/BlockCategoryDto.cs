namespace OpenBullet2.Web.Dtos.Config.Blocks;

/// <summary>
/// Description about the category of a block.
/// </summary>
public class BlockCategoryDto
{
    /// <summary>
    /// The name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The path of the category.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The namespace of the category.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// The description of the category.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The background color.
    /// </summary>
    public string BackgroundColor { get; set; } = string.Empty;

    /// <summary>
    /// The foreground color.
    /// </summary>
    public string ForegroundColor { get; set; } = string.Empty;
}
