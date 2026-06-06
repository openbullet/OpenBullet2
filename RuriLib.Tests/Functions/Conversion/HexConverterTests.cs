using RuriLib.Functions.Conversion;
using Xunit;

namespace RuriLib.Tests.Functions.Conversion;

public class HexConverterTests
{
    private readonly string threeCharactersString = "A16";
    private readonly string fourCharactersString = "A16F";

    [Fact]
    public void ToByteArray_BackAndForth_NoChange()
    {
        var bytes = HexConverter.ToByteArray(fourCharactersString);
        var hex = HexConverter.ToHexString(bytes);
        Assert.Equal(fourCharactersString, hex, true);
    }

    [Fact]
    public void ToByteArray_ThreeCharacterString_AutoPad()
    {
        var bytes = HexConverter.ToByteArray(threeCharactersString);
        Assert.Equal([0xA, 0x16], bytes);
    }
}
