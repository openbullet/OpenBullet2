using System;
using RuriLib.Models.Blocks;
using Xunit;

namespace RuriLib.Tests.Models.Blocks;

public class BlockDescriptorTests
{
    [Fact]
    public void Constructor_DefaultCollectionsAndStrings_AreInitialized()
    {
        var descriptor = new BlockDescriptor();

        Assert.Equal(string.Empty, descriptor.Id);
        Assert.Equal(string.Empty, descriptor.Name);
        Assert.NotNull(descriptor.Aliases);
        Assert.Empty(descriptor.Aliases);
        Assert.Equal(string.Empty, descriptor.Description);
        Assert.Equal(string.Empty, descriptor.ExtraInfo);
        Assert.Equal(string.Empty, descriptor.AssemblyFullName);
        Assert.NotNull(descriptor.Parameters);
        Assert.Empty(descriptor.Parameters);
        Assert.NotNull(descriptor.Actions);
        Assert.Empty(descriptor.Actions);
        Assert.NotNull(descriptor.Images);
        Assert.Empty(descriptor.Images);
        Assert.Equal(string.Empty, descriptor.Category.Name);
    }

    [Fact]
    public void BlockActionInfo_DefaultDelegate_IsNoOp()
    {
        var action = new BlockActionInfo();
        var block = new LoliCodeBlockInstance(new LoliCodeBlockDescriptor());

        var exception = Record.Exception(() => action.Delegate(block));

        Assert.Null(exception);
        Assert.Equal(string.Empty, action.Name);
        Assert.Equal(string.Empty, action.Description);
    }

    [Fact]
    public void BlockImageInfo_DefaultValue_IsEmptyByteArray()
    {
        var image = new BlockImageInfo();

        Assert.Equal(string.Empty, image.Name);
        Assert.NotNull(image.Value);
        Assert.Empty(image.Value);
        Assert.Equal(0, image.MaxWidth);
        Assert.Equal(0, image.MaxHeight);
    }
}
