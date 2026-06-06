using RuriLib.Functions.Conversion;
using Xunit;

namespace RuriLib.Tests.Functions.Conversion;

public class BinaryConverterTests
{
    private readonly string fourCharactersString = "1101";
    private readonly string eightCharactersString = "00101101";

    [Fact]
    public void ToByteArray_BackAndForth_NoChange()
    {
        var bytes = BinaryConverter.ToByteArray(eightCharactersString);
        var hex = BinaryConverter.ToBinaryString(bytes);
        Assert.Equal(eightCharactersString, hex, true);
    }

    [Fact]
    public void ToByteArray_FourCharacterString_AutoPad()
    {
        var bytes = BinaryConverter.ToByteArray(fourCharactersString);
        Assert.Equal([13], bytes);
    }
}
