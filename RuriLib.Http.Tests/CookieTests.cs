using System;
using System.Linq;
using System.Net;
using Xunit;

namespace RuriLib.Http.Tests;

public class CookieTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("undefined")]
    [InlineData("cookie1")]
    [InlineData(";")]
    public void SetCookie_EmptyString_DoNothing(string cookie)
    {
        var cookies = new CookieContainer();
        var uri = new Uri("http://example.com");

        HttpResponseMessageBuilder.SetCookie(cookie, cookies, uri);

        var cookieCollection = cookies.GetCookies(uri);
        Assert.Empty(cookieCollection);
    }

    [Theory]
    [InlineData("cookie1=value1")]
    [InlineData("cookie1=value1;")]
    [InlineData("cookie1=value1; ")]
    [InlineData("cookie1=value1; Domain=example.com; Path=/; Secure; HttpOnly")]
    public void SetCookie_SingleCookie_SetSuccessfully(string cookie)
    {
        var cookies = new CookieContainer();
        var uri = new Uri("http://example.com");

        HttpResponseMessageBuilder.SetCookie(cookie, cookies, uri);

        var cookieCollection = cookies.GetCookies(uri);
        Assert.Single(cookieCollection);
        Assert.Equal("cookie1", cookieCollection[0].Name);
        Assert.Equal("value1", cookieCollection[0].Value);
    }

    [Theory]
    [InlineData("cookie1=value1, cookie2=value2")]
    [InlineData("cookie1=value1, cookie2=value2;")]
    [InlineData("cookie1=value1, cookie2=value2; ")]
    [InlineData("cookie1=value1, cookie2=value2, ")]
    [InlineData("cookie1=value1, cookie2=value2, undefined")]
    [InlineData("cookie1=value1, undefined, cookie2=value2")]
    [InlineData("cookie1=value1; Domain=example.com; Path=/; Secure; HttpOnly, cookie2=value2")]
    [InlineData("cookie1=value1; Domain=example.com; Path=/; Secure; HttpOnly, cookie2=value2; Domain=example.com; Path=/; Secure; HttpOnly")]
    public void SetCookie_MultipleCookies_SetSuccessfully(string cookies)
    {
        var cookiesContainer = new CookieContainer();
        var uri = new Uri("http://example.com");

        HttpResponseMessageBuilder.SetCookies(cookies, cookiesContainer, uri);

        var cookieCollection = cookiesContainer.GetCookies(uri);
        Assert.Equal(2, cookieCollection.Count);

        Assert.Equal("cookie1", cookieCollection[0].Name);
        Assert.Equal("value1", cookieCollection[0].Value);

        Assert.Equal("cookie2", cookieCollection[1].Name);
        Assert.Equal("value2", cookieCollection[1].Value);
    }

    [Theory]
    [InlineData("cookie1=value1; Expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/")]
    [InlineData("SOCS=CAAaBgiAlf_PBg; Expires=Tue, 09 Dec 2025 18:12:05 GMT; Path=/; Domain=.google.com; Secure; SameSite=Lax")]
    public void SetCookie_WithExpiresAttributeContainingComma_SetSuccessfully(string cookie)
    {
        var cookies = new CookieContainer();
        var uri = new Uri("https://www.google.com");

        HttpResponseMessageBuilder.SetCookies(cookie, cookies, uri);

        var cookieCollection = cookies.GetCookies(uri);
        Assert.Single(cookieCollection);
    }

    [Fact]
    public void SetCookie_MultipleCookiesWithExpiresAttribute_SetSuccessfully()
    {
        const string cookiesHeader =
            "cookie1=value1; Expires=Wed, 09 Jun 2027 10:18:14 GMT; Path=/, cookie2=value2";

        var cookies = new CookieContainer();
        var uri = new Uri("http://example.com");

        HttpResponseMessageBuilder.SetCookies(cookiesHeader, cookies, uri);

        var cookieCollection = cookies.GetCookies(uri);
        Assert.Equal(2, cookieCollection.Count);
        Assert.Equal("cookie1", cookieCollection[0].Name);
        Assert.Equal("value1", cookieCollection[0].Value);
        Assert.Equal("cookie2", cookieCollection[1].Name);
        Assert.Equal("value2", cookieCollection[1].Value);
    }

    [Fact]
    public void SetCookie_MultipleCookiesWithQuotedCommaAttributes_SetSuccessfully()
    {
        const string cookiesHeader =
            "cookie1=value1; Comment=\"hello, world\"; Version=1, cookie2=value2; Port=\"80,8080\"";

        var cookies = new CookieContainer();
        var uri = new Uri("http://example.com");

        HttpResponseMessageBuilder.SetCookies(cookiesHeader, cookies, uri);

        var cookieCollection = cookies.GetCookies(uri);
        Assert.Equal(2, cookieCollection.Count);
        Assert.Equal("cookie1", cookieCollection[0].Name);
        Assert.Equal("value1", cookieCollection[0].Value);
        Assert.Equal("cookie2", cookieCollection[1].Name);
        Assert.Equal("value2", cookieCollection[1].Value);
    }

    [Fact]
    public void SetCookie_ExpiredCookie_MarksExistingCookieAsExpired()
    {
        const string aliveCookie = "cookie1=value1";
        const string expiredCookie = "cookie1=deleted; Expires=Wed, 09 Jun 2021 10:18:14 GMT; Path=/";

        var cookies = new CookieContainer();
        var uri = new Uri("http://example.com");

        HttpResponseMessageBuilder.SetCookie(aliveCookie, cookies, uri);
        HttpResponseMessageBuilder.SetCookies(expiredCookie, cookies, uri);

        var cookieCollection = cookies.GetCookies(uri);
        Assert.Empty(cookieCollection.Cast<Cookie>());
    }
}
