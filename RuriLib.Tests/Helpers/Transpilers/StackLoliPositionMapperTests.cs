using RuriLib.Helpers.Blocks;
using RuriLib.Helpers.Transpilers;
using RuriLib.Models.Blocks;
using System.Collections.Generic;
using Xunit;

namespace RuriLib.Tests.Helpers.Transpilers;

public class StackLoliPositionMapperTests
{
    [Fact]
    public void GetBlockIndexAtLine_MapsRawLinesAndBlankSeparatorToExpectedBlocks()
    {
        var script = string.Join('\n',
        [
            "PRINT \"alpha\"",
            "PRINT \"beta\"",
            "",
            "BLOCK:ConstantString",
            "  => VAR \"result\"",
            "ENDBLOCK",
            "",
            "BLOCK:ConstantString",
            "  => VAR \"next\"",
            "ENDBLOCK"
        ]);

        Assert.Equal(0, StackLoliPositionMapper.GetBlockIndexAtLine(script, 1));
        Assert.Equal(0, StackLoliPositionMapper.GetBlockIndexAtLine(script, 2));
        Assert.Equal(1, StackLoliPositionMapper.GetBlockIndexAtLine(script, 3));
        Assert.Equal(1, StackLoliPositionMapper.GetBlockIndexAtLine(script, 4));
        Assert.Equal(1, StackLoliPositionMapper.GetBlockIndexAtLine(script, 6));
        Assert.Equal(2, StackLoliPositionMapper.GetBlockIndexAtLine(script, 7));
        Assert.Equal(2, StackLoliPositionMapper.GetBlockIndexAtLine(script, 8));
    }

    [Fact]
    public void GetBlockIndexAtLine_MapsTrailingWhitespaceToPreviousBlock()
    {
        var script = string.Join('\n',
        [
            "BLOCK:ConstantString",
            "  => VAR \"result\"",
            "ENDBLOCK",
            "",
            ""
        ]);

        Assert.Equal(0, StackLoliPositionMapper.GetBlockIndexAtLine(script, 4));
        Assert.Equal(0, StackLoliPositionMapper.GetBlockIndexAtLine(script, 5));
    }

    [Fact]
    public void GetLineNumberForBlock_ReturnsFirstGeneratedLineForEachBlock()
    {
        var blocks = new List<BlockInstance>
        {
            new LoliCodeBlockInstance(new LoliCodeBlockDescriptor())
            {
                Script = "PRINT \"first\"\nPRINT \"second\"\n"
            },
            BlockFactory.GetBlock<BlockInstance>("ConstantString"),
            BlockFactory.GetBlock<BlockInstance>("ConstantString")
        };

        var script = Stack2LoliTranspiler.Transpile(blocks);
        var lines = script.Split(["\r\n", "\n"], System.StringSplitOptions.None);

        var rawLine = StackLoliPositionMapper.GetLineNumberForBlock(blocks, 0);
        var firstStructuredLine = StackLoliPositionMapper.GetLineNumberForBlock(blocks, 1);
        var secondStructuredLine = StackLoliPositionMapper.GetLineNumberForBlock(blocks, 2);

        Assert.Equal(1, rawLine);
        Assert.Equal("PRINT \"first\"", lines[rawLine!.Value - 1]);
        Assert.StartsWith("BLOCK:ConstantString", lines[firstStructuredLine!.Value - 1]);
        Assert.StartsWith("BLOCK:ConstantString", lines[secondStructuredLine!.Value - 1]);
        Assert.True(secondStructuredLine > firstStructuredLine);
    }
}
