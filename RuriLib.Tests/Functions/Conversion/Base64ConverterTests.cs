using RuriLib.Functions.Conversion;
using System.Text;
using Xunit;

namespace RuriLib.Tests.Functions.Conversion
{
    public class Base64ConverterTests
    {
        [Fact]
        public void ToByteArray_DottedString_RemoveDotsAndDecode()
        {
            byte[] bytes = Base64Converter.ToByteArray("aGVsbG8gaG93I.GFyZSB5b3U/");
            Assert.Equal("hello how are you?", Encoding.UTF8.GetString(bytes));
        }
    }
}
