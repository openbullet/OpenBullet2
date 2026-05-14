using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Trees;
using RuriLib.Models.Variables;
using Xunit;

namespace RuriLib.Tests.Helpers.Blocks;

public class DescriptorsRepositoryTests
{
    [Fact]
    public void GetAs_ExistingDescriptor_ReturnsTypedDescriptor()
    {
        var repository = new DescriptorsRepository();

        var descriptor = repository.GetAs<HttpRequestBlockDescriptor>("HttpRequest");

        Assert.Equal("HttpRequest", descriptor.Id);
    }

    [Fact]
    public void GetAs_WrongType_ThrowsInvalidCastException()
    {
        var repository = new DescriptorsRepository();

        Assert.Throws<InvalidCastException>(() => repository.GetAs<LoliCodeBlockDescriptor>("ConstantString"));
    }

    [Fact]
    public void ToVariableType_TaskString_ReturnsString()
        => Assert.Equal(VariableType.String, DescriptorsRepository.ToVariableType(typeof(Task<string>)));

    [Fact]
    public void ToVariableType_InvalidType_Throws()
        => Assert.Throws<InvalidCastException>(() => DescriptorsRepository.ToVariableType(typeof(DateTime)));

    [Fact]
    public void GetAs_BlockIdOverride_UsesStableIdAndAsyncMethodName()
    {
        var repository = new DescriptorsRepository();

        var descriptor = repository.GetAs<AutoBlockDescriptor>("FileExists");

        Assert.Equal("FileExists", descriptor.Id);
        Assert.Equal("FileExistsAsync", descriptor.MethodName);
        Assert.True(descriptor.Async);
    }

    [Fact]
    public void GetAs_DnsLookup_ReturnsAutoDescriptor()
    {
        var repository = new DescriptorsRepository();

        var descriptor = repository.GetAs<AutoBlockDescriptor>("DnsLookup");

        Assert.Equal("DnsLookup", descriptor.Id);
        Assert.Equal("LookupDnsAsync", descriptor.MethodName);
        Assert.True(descriptor.Async);
    }

    [Fact]
    public void AsTree_ContainsAutoBlockDescriptors()
    {
        var repository = new DescriptorsRepository();

        var tree = repository.AsTree();

        Assert.True(tree.IsRoot);
        Assert.NotEmpty(tree.SubCategories);
        Assert.Contains(Flatten(tree), descriptor => descriptor.Id == "ConstantString");
    }

    [Fact]
    public void AsTree_ParentCategoriesUseScopedMetadata()
    {
        var repository = new DescriptorsRepository();

        var tree = repository.AsTree();
        var requestsCategory = tree.SubCategories
            .First(sc => sc.Name == "RuriLib")
            .SubCategories.First(sc => sc.Name == "Blocks")
            .SubCategories.First(sc => sc.Name == "Requests")
            .Category;

        Assert.Equal("Requests", requestsCategory.Name);
        Assert.Equal("Blocks for performing network requests",
            requestsCategory.Description);
    }

    private static IEnumerable<BlockDescriptor> Flatten(CategoryTreeNode node)
        => node.Descriptors.Concat(node.SubCategories.SelectMany(Flatten));
}
