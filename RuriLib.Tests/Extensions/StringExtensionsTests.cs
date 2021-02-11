using RuriLib.Extensions;
using Xunit;

namespace RuriLib.Tests.Extensions
{
    public class StringExtensionsTests
    {
        [Fact]
        public void PadLeftToNearestMultiple_NormalTest_Pad()
        {
            var padded = "ABCD".PadLeftToNearestMultiple(8, 'E');
            Assert.Equal("EEEEABCD", padded);
        }

        [Fact]
        public void WithEnding_NormalTest_CorrectResult()
        {
            Assert.Equal("hello", "hel".WithEnding("llo"));
        }

        [Fact]
        public void RightMostCharacters_NormalTest_CorrectResult()
        {
            Assert.Equal("llo", "hello".RightMostCharacters(3));
        }

        [Fact]
        public void IsSubPathOf_ValidSubPath_True()
        {
            Assert.True("C:/test/dir/file.txt".IsSubPathOf("C:/test/"));
        }

        [Theory]
        [InlineData("C:/windows/system32/cmd.exe")]
        [InlineData("C:/test/../windows/system32/cmd.exe")]
        public void IsSubPathOf_IllegalSubPath_False(string path)
        {
            Assert.False(path.IsSubPathOf("C:/test/"));
        }
    }
}
