using System.Text;
using RuriLib.Functions.Conversion;
using Xunit;

namespace RuriLib.Tests.Functions.Conversion;

public class Base64ConverterTests
{
    [Fact]
    public void ToByteArray_DottedString_RemoveDotsAndDecode()
    {
        var bytes = Base64Converter.ToByteArray("aGVsbG8gaG93I.GFyZSB5b3U/");
        Assert.Equal("hello how are you?", Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void ToBase64String_UrlEncoded_RoundTrips()
    {
        var bytes = Encoding.UTF8.GetBytes("hello+/=_world");

        var encoded = Base64Converter.ToBase64String(bytes, urlEncoded: true);
        var decoded = Base64Converter.ToByteArray(encoded, urlEncoded: true);

        Assert.Equal(bytes, decoded);
    }
}
