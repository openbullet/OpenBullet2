using RuriLib.Functions.Http;
using Xunit;

namespace RuriLib.Tests.Functions.Http;

public class HttpClientRequestHandlerTests
{
    [Fact]
    public void TryParseCookie_AttributesWithoutCookiePair_ReturnsFalse()
    {
        var parsed = HttpClientRequestHandler.TryParseCookie(
            "HttpOnly;Secure;SameSite=None;",
            out var cookieName,
            out var cookieValue);

        Assert.False(parsed);
        Assert.Null(cookieName);
        Assert.Null(cookieValue);
    }
}
