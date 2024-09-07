using System;
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
}
