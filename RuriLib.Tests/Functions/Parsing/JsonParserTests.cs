using System.Linq;
using RuriLib.Functions.Parsing;
using Xunit;

namespace RuriLib.Tests.Functions.Parsing;

public class JsonParserTests
{
    private readonly string jsonObject = "{ \"key\": \"value\" }";
    private readonly string jsonArray = "[ \"elem1\", \"elem2\" ]";

    [Fact]
    public void GetValuesByKey_GetStringFromObject_ReturnValue()
    {
        var match = JsonParser.GetValuesByKey(jsonObject, "key").FirstOrDefault();
        Assert.Equal("value", match);
    }

    [Fact]
    public void GetValuesByKey_MissingKey_MatchNothing()
    {
        var match = JsonParser.GetValuesByKey(jsonObject, "dummy").FirstOrDefault();
        Assert.Null(match);
    }

    [Fact]
    public void GetValuesByKey_GetStringsFromArray_ReturnValues()
    {
        var match = JsonParser.GetValuesByKey(jsonArray, "[*]").ToArray();
        Assert.Equal(new[] { "elem1", "elem2" }, match);
    }

    [Fact]
    public void GetValuesByKey_GetIntegerFromObject_ReturnValue()
    {
        var match = JsonParser.GetValuesByKey("{ \"user_id\": 123 }", "user_id").FirstOrDefault();
        Assert.Equal("123", match);
    }

    [Fact]
    public void GetValuesByKey_GetFloatFromObject_UsesInvariantCulture()
    {
        var match = JsonParser.GetValuesByKey("{ \"ratio\": 1.5 }", "ratio").FirstOrDefault();
        Assert.Equal("1.5", match);
    }

    [Fact]
    public void GetValuesByKey_LeadingWhitespaceObject_ReturnValue()
    {
        var match = JsonParser.GetValuesByKey("  \r\n\t{ \"key\": \"value\" }", "key").FirstOrDefault();
        Assert.Equal("value", match);
    }

    [Fact]
    public void GetValuesByKey_BomPrefixedObject_ReturnValue()
    {
        var match = JsonParser.GetValuesByKey("\uFEFF{ \"key\": \"value\" }", "key").FirstOrDefault();
        Assert.Equal("value", match);
    }

    [Fact]
    public void GetValuesByKey_EscapedUnicodeString_ReturnValue()
    {
        var match = JsonParser.GetValuesByKey("{ \"name\": \"Jos\\u00E9\" }", "name").FirstOrDefault();
        Assert.Equal("Jos\u00E9", match);
    }
}
