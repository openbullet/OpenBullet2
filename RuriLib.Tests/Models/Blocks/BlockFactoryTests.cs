using System;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using Xunit;

namespace RuriLib.Tests.Models.Blocks;

public class BlockFactoryTests
{
    [Fact]
    public void GetBlock_WrongType_ThrowsInvalidCastException()
        => Assert.Throws<InvalidCastException>(() => BlockFactory.GetBlock<LoliCodeBlockInstance>("ConstantString"));
}
