using System.Linq;
using Xunit;

namespace RuriLib.Http.Tests;

public class CookieHeaderParserTests
{
    public static TheoryData<string, string[]> SplitCookiesCases => new()
    {
        { "", [] },
        { "   ", [] },
        {
            "cookie1=value1",
            ["cookie1=value1"]
        },
        {
            "cookie1=value1, cookie2=value2",
            ["cookie1=value1", "cookie2=value2"]
        },
        {
            "cookie1=value1; Expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/",
            ["cookie1=value1; Expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/"]
        },
        {
            "cookie1=value1; expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/, cookie2=value2",
            ["cookie1=value1; expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/", "cookie2=value2"]
        },
        {
            "cookie1=value1; Comment=\"hello, world\"; Version=1, cookie2=value2",
            ["cookie1=value1; Comment=\"hello, world\"; Version=1", "cookie2=value2"]
        },
        {
            "cookie1=value1; Port=\"80,8080\"; Version=1, cookie2=value2",
            ["cookie1=value1; Port=\"80,8080\"; Version=1", "cookie2=value2"]
        },
        {
            "cookie1=value1; Comment=\"hello, world\"; Expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/, cookie2=value2; Port=\"80,8080\"",
            [
                "cookie1=value1; Comment=\"hello, world\"; Expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/",
                "cookie2=value2; Port=\"80,8080\""
            ]
        },
        {
            " cookie1=value1 ; Path=/ ,  cookie2=value2; SameSite=None; Secure ",
            ["cookie1=value1 ; Path=/", "cookie2=value2; SameSite=None; Secure"]
        }
    };

    [Theory]
    [MemberData(nameof(SplitCookiesCases))]
    public void SplitCookies_ParsesExpectedCookieBoundaries(string header, string[] expected)
    {
        var actual = CookieHeaderParser.SplitCookies(header).ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SplitCookies_DoesNotTreatCookieValueContainingExpiresSubstringAsExpiresAttribute()
    {
        const string header = "cookie1=myexpires=value1, cookie2=value2";

        var actual = CookieHeaderParser.SplitCookies(header).ToArray();

        Assert.Equal(["cookie1=myexpires=value1", "cookie2=value2"], actual);
    }
}
