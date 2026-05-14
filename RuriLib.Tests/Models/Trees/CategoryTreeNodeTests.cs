using System;
using RuriLib.Models.Blocks;
using RuriLib.Models.Trees;
using Xunit;

namespace RuriLib.Tests.Models.Trees;

public class CategoryTreeNodeTests
{
    [Fact]
    public void Category_WithResolvedCategory_ReturnsResolvedCategory()
    {
        var node = new CategoryTreeNode
        {
            ResolvedCategory = new BlockCategory
            {
                Name = "Requests",
                Description = "Blocks for performing network requests"
            },
            Descriptors =
            [
                new BlockDescriptor
                {
                    Category = new BlockCategory
                    {
                        Name = "DNS",
                        Description = "Blocks to query DNS records"
                    }
                }
            ]
        };

        Assert.Equal("Requests", node.Category.Name);
        Assert.Equal("Blocks for performing network requests",
            node.Category.Description);
    }

    [Fact]
    public void Category_WithDescriptor_ReturnsDescriptorCategory()
    {
        var category = new BlockCategory { Name = "Leaf", Path = "A.B" };
        var node = new CategoryTreeNode
        {
            Descriptors =
            [
                new BlockDescriptor { Category = category }
            ]
        };

        Assert.Equal(category.Name, node.Category.Name);
        Assert.Equal(category.Path, node.Category.Path);
    }

    [Fact]
    public void Category_WithOnlySubcategories_ReusesChildCategoryAndOverridesName()
    {
        var node = new CategoryTreeNode
        {
            Name = "Parent",
            SubCategories =
            [
                new CategoryTreeNode
                {
                    Name = "Child",
                    Descriptors =
                    [
                        new BlockDescriptor
                        {
                            Category = new BlockCategory
                            {
                                Name = "Child",
                                Path = "Functions.String"
                            }
                        }
                    ]
                }
            ]
        };

        var category = node.Category;

        Assert.Equal("Parent", category.Name);
        Assert.Equal("Functions.String", category.Path);
    }

    [Fact]
    public void Category_WithoutDescriptorsOrSubcategories_Throws()
    {
        var node = new CategoryTreeNode();

        Assert.Throws<InvalidOperationException>(() => _ = node.Category);
    }
}
