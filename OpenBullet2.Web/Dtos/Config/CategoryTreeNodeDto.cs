namespace OpenBullet2.Web.Dtos.Config;

/// <summary>
/// DTO that contains information about a node
/// in the block category tree.
/// </summary>
public class CategoryTreeNodeDto
{
    /// <summary>
    /// The name of the category.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The subcategories.
    /// </summary>
    public required IEnumerable<CategoryTreeNodeDto> SubCategories { get; set; }

    /// <summary>
    /// The IDs of the blocks in the category.
    /// </summary>
    public required IEnumerable<string> DescriptorIds { get; set; }
}
