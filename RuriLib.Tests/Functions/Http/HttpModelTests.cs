using RuriLib.Functions.Http;
using RuriLib.Functions.Http.Options;
using RuriLib.Functions.Networking;
using RuriLib.Http.Curl;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Proxies;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Http;

public class HttpModelTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public void HttpOptions_HaveExpectedDefaults()
    {
        var options = new HttpOptions();

        Assert.Equal(5, options.ConnectTimeout.TotalSeconds);
        Assert.Equal(10, options.ReadWriteTimeout.TotalSeconds);
        Assert.True(options.AutoRedirect);
        Assert.Equal(8, options.MaxNumberOfRedirects);
        Assert.True(options.ReadResponseContent);
        Assert.Equal(SecurityProtocol.SystemDefault, options.SecurityProtocol);
        Assert.True(options.IgnoreCertificateValidation);
        Assert.False(options.UseCustomCipherSuites);
        Assert.NotEmpty(options.CustomCipherSuites);
        Assert.Equal(CurlImpersonateBrowserProfile.Chrome142, options.CurlImpersonateBrowserProfile);
        Assert.True(options.CurlUseBrowserHeaders);
    }

    [Fact]
    public void HttpRequestOptions_HaveExpectedDefaults()
    {
        var options = new HttpRequestOptions();

        Assert.Equal(string.Empty, options.Url);
        Assert.Equal(HttpMethod.GET, options.Method);
        Assert.True(options.AutoRedirect);
        Assert.Equal(8, options.MaxNumberOfRedirects);
        Assert.Equal(HttpLibrary.RuriLibHttp, options.HttpLibrary);
        Assert.Equal(SecurityProtocol.SystemDefault, options.SecurityProtocol);
        Assert.True(options.IgnoreCertificateValidation);
        Assert.Empty(options.CustomCookies);
        Assert.Empty(options.CustomHeaders);
        Assert.Equal(10000, options.TimeoutMilliseconds);
        Assert.Equal("1.1", options.HttpVersion);
        Assert.False(options.UseCustomCipherSuites);
        Assert.Empty(options.CustomCipherSuites);
        Assert.Equal(string.Empty, options.CodePagesEncoding);
        Assert.False(options.AlwaysSendContent);
        Assert.False(options.DecodeHtml);
        Assert.True(options.ReadResponseContent);
        Assert.Equal(CurlImpersonateBrowserProfile.Chrome142, options.CurlImpersonateBrowserProfile);
        Assert.True(options.CurlUseBrowserHeaders);
    }

    [Fact]
    public void HttpRequestDerivedOptions_HaveExpectedDefaults()
    {
        var standard = new StandardHttpRequestOptions();
        var raw = new RawHttpRequestOptions();
        var basicAuth = new BasicAuthHttpRequestOptions();
        var multipart = new MultipartHttpRequestOptions();

        Assert.Equal(string.Empty, standard.Content);
        Assert.Equal(string.Empty, standard.ContentType);
        Assert.False(standard.UrlEncodeContent);

        Assert.Empty(raw.Content);
        Assert.Equal(string.Empty, raw.ContentType);

        Assert.Equal(string.Empty, basicAuth.Username);
        Assert.Equal(string.Empty, basicAuth.Password);

        Assert.Equal(string.Empty, multipart.Boundary);
        Assert.Empty(multipart.Contents);
    }

    [Fact]
    public void HostEntry_ConstructorAssignsHostAndPort()
    {
        var entry = new HostEntry("example.com", 443);

        Assert.Equal("example.com", entry.Host);
        Assert.Equal(443, entry.Port);
    }

    [Fact]
    public void GetCurlImpersonateHandlerOptions_DisablesConnectTimeoutWithoutProxy()
    {
        var options = new HttpOptions
        {
            ConnectTimeout = TimeSpan.FromMilliseconds(1234)
        };

        var handlerOptions = HttpFactory.GetCurlImpersonateHandlerOptions(null, options, new CookieContainer());

        Assert.Equal(Timeout.InfiniteTimeSpan, handlerOptions.ConnectTimeout);
        Assert.Equal(Timeout.InfiniteTimeSpan, handlerOptions.Timeout);
        Assert.Null(handlerOptions.ProxyUri);
    }

    [Fact]
    public void GetCurlImpersonateHandlerOptions_KeepsConnectTimeoutWithProxy()
    {
        var proxy = new Proxy("127.0.0.1", 8080, ProxyType.Http);
        var options = new HttpOptions
        {
            ConnectTimeout = TimeSpan.FromMilliseconds(1234)
        };

        var handlerOptions = HttpFactory.GetCurlImpersonateHandlerOptions(proxy, options, new CookieContainer());

        Assert.Equal(TimeSpan.FromMilliseconds(1234), handlerOptions.ConnectTimeout);
        Assert.Equal(new Uri("http://127.0.0.1:8080"), handlerOptions.ProxyUri);
    }

    [Fact]
    public async Task CreateMultipartContent_StringWithoutContentType_OmitsHeader()
    {
        using var content = HttpRequestHandler.CreateMultipartContent(
            new StringHttpContent("field", "hello", string.Empty));

        Assert.Null(content.Headers.ContentType);
        Assert.Equal("hello", await content.ReadAsStringAsync(TestCancellationToken));
    }

    [Fact]
    public void CreateMultipartContent_RawWithoutContentType_OmitsHeader()
    {
        using var content = HttpRequestHandler.CreateMultipartContent(
            new RawHttpContent("field", Encoding.UTF8.GetBytes("hello"), string.Empty));

        Assert.Null(content.Headers.ContentType);
    }

    [Fact]
    public void CreateMultipartContent_FileWithoutContentType_OmitsHeader()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("hello"));
        using var content = HttpRequestHandler.CreateMultipartContent(
            new FileHttpContent("field", "hello.txt", string.Empty), stream);

        Assert.Null(content.Headers.ContentType);
    }

}
