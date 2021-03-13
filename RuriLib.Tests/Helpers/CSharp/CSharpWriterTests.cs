using RuriLib.Helpers.CSharp;
using Xunit;

namespace RuriLib.Tests.Helpers.CSharp
{
    public class CSharpWriterTests
    {
        [Fact]
        public void SerializeInterpString_Alone_ReplaceCorrectly()
        {
            Assert.Equal("$\"{value}\"", CSharpWriter.SerializeInterpString("<value>"));
        }

        [Fact]
        public void SerializeInterpString_Surrounded_ReplaceCorrectly()
        {
            Assert.Equal("$\"my {value} is cool\"", CSharpWriter.SerializeInterpString("my <value> is cool"));
        }

        [Fact]
        public void SerializeInterpString_SingleCharacter_ReplaceCorrectly()
        {
            Assert.Equal("$\"my {a} is cool\"", CSharpWriter.SerializeInterpString("my <a> is cool"));
        }
    }
}
