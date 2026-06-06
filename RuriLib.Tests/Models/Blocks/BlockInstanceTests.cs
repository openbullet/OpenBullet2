using System;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using Xunit;

namespace RuriLib.Tests.Models.Blocks;

public class BlockInstanceTests
{
    private readonly string _nl = Environment.NewLine;

    [Fact]
    public void FromLC_CommonMetadata_IsParsedBeforeDerivedSettings()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");
        var script = $"DISABLED{_nl}LABEL:My Label{_nl}  value = \"hello\"{_nl}";
        var lineNumber = 0;

        block.FromLC(ref script, ref lineNumber);

        Assert.True(block.Disabled);
        Assert.Equal("My Label", block.Label);
        Assert.Equal("hello", Assert.IsType<StringSetting>(block.Settings["value"].FixedSetting).Value);
        Assert.Equal($"  value = \"hello\"{_nl}", script);
    }

    [Fact]
    public void GetFixedSetting_MatchingType_ReturnsSetting()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");

        var setting = block.GetFixedSetting<StringSetting>("value");

        Assert.NotNull(setting);
        Assert.Equal(string.Empty, setting!.Value);
    }

    [Fact]
    public void GetFixedSetting_WrongType_ReturnsNull()
    {
        var block = BlockFactory.GetBlock<AutoBlockInstance>("ConstantString");

        Assert.Null(block.GetFixedSetting<IntSetting>("value"));
    }
}
