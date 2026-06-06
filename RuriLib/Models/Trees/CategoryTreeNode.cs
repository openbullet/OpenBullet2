using System.Collections.Generic;
using System.Linq;
using System;
using RuriLib.Models.Blocks;

namespace RuriLib.Models.Trees;

/// <summary>
/// A node in the hierarchical block category tree.
/// </summary>
public class CategoryTreeNode
{
    /// <summary>
    /// The resolved category metadata for this node, when available.
    /// </summary>
    public BlockCategory? ResolvedCategory { get; set; }

    /// <summary>
    /// The parent category node, if any.
    /// </summary>
    public CategoryTreeNode? Parent { get; set; }

    /// <summary>
    /// The child categories of this node.
    /// </summary>
    public List<CategoryTreeNode> SubCategories { get; set; } = [];

    /// <summary>
    /// The descriptors that belong directly to this node.
    /// </summary>
    public List<BlockDescriptor> Descriptors { get; set; } = [];

    /// <summary>
    /// The category name represented by the node.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this node is the root of the tree.
    /// </summary>
    public bool IsRoot => Parent is null;

    /// <summary>
    /// Gets the category metadata represented by this node.
    /// </summary>
    public BlockCategory Category
    {
        get
        {
            if (ResolvedCategory is { } resolvedCategory)
            {
                return resolvedCategory;
            }

            if (Descriptors.Count > 0)
            {
                return Descriptors.First().Category;
            }

            if (SubCategories.Count == 0)
            {
                throw new InvalidOperationException("Cannot resolve the category of an empty tree node");
            }

            var category = SubCategories.First().Category;
            category.Name = Name;
            return category;
        }
    }
}
