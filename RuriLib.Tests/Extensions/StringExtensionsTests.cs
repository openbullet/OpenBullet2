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

        [Theory]
        [InlineData("C:/test/dir/file.txt")]
        [InlineData("C:\\test\\dir\\file.txt")]
        [InlineData("file.txt")]
        [InlineData("./file.txt")]
        [InlineData("Test/file.txt")]
        public void IsSubPathOf_ValidSubPath_True(string path)
        {
            Assert.True(path.IsSubPathOf("C:/test/"));
        }

        [Theory]
        [InlineData("/home/user/file.txt")]
        [InlineData("file.txt")]
        [InlineData("./file.txt")]
        [InlineData("Test/file.txt")]
        public void IsSubPathOf_ValidSubPathUnix_True(string path)
        {
            Assert.True(path.IsSubPathOf("/home/user/"));
        }

        [Theory]
        [InlineData("C:/windows/system32/cmd.exe")]
        [InlineData("C:/test/../windows/system32/cmd.exe")]
        [InlineData("../../windows/system32/cmd.exe")]
        public void IsSubPathOf_IllegalSubPath_False(string path)
        {
            Assert.False(path.IsSubPathOf("C:/test/"));
        }

        [Theory]
        [InlineData("/opt/test")]
        [InlineData("/home/user/../../root")]
        [InlineData("../../root")]
        public void IsSubPathOf_IllegalSubPathUnix_False(string path)
        {
            Assert.False(path.IsSubPathOf("/home/user/"));
        }
    }
}
