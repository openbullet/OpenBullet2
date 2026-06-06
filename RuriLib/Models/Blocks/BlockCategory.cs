namespace RuriLib.Models.Blocks;

/// <summary>
/// Represents the category metadata attached to a block.
/// </summary>
public struct BlockCategory
{
    /// <summary>
    /// Initializes a new empty <see cref="BlockCategory"/>.
    /// </summary>
    public BlockCategory()
    {
    }

    /// <summary>
    /// The display name of the category.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The hierarchical category path.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// The namespace that identifies the category.
    /// </summary>
    public string Namespace { get; set; } = string.Empty;

    /// <summary>
    /// The category description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The background color shown in the UI.
    /// </summary>
    public string BackgroundColor { get; set; } = string.Empty;

    /// <summary>
    /// The foreground color shown in the UI.
    /// </summary>
    public string ForegroundColor { get; set; } = string.Empty;
}
