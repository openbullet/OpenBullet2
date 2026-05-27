using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuriLib.Blocks.Requests.Http;
using RuriLib.Functions.Http;
using RuriLib.Functions.Http.Options;
using RuriLib.Logging;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using Xunit;

namespace RuriLib.Tests.Functions.Http;

public class HttpTests
{
    private static BotData NewBotData() => new(
        new(null!)
        {
            ProxySettings = new MockedProxySettingsProvider(),
            Security = new MockedSecurityProvider()
        },
        new ConfigSettings(),
        new BotLogger(),
        new DataLine("", new WordlistType()),
        null,
        false);

    [Theory]
    [InlineData(HttpLibrary.RuriLibHttp)]
    [InlineData(HttpLibrary.SystemNet)]
    [InlineData(HttpLibrary.CurlImpersonate)]
    public async Task HttpRequestStandard_Get_Verify(HttpLibrary library)
    {
        var httpBin = await TestHttpBin.BuildUrl("anything");
        var data = NewBotData();

        var cookies = new Dictionary<string, string>
        {
            { "name1", "value1" },
            { "name2", "value2" }
        };

        var headers = new Dictionary<string, string>
        {
            { "Custom", "value" }
        };

        var options = new StandardHttpRequestOptions
        {
            Url = httpBin,
            Method = HttpMethod.GET,
            HttpLibrary = library,
            CustomHeaders = headers,
            CustomCookies = cookies
        };

        await Methods.HttpRequestStandard(data, options);

        Assert.Contains(data.Logger.Entries,
            entry => entry.Message.StartsWith("Response HTTP version: HTTP/", StringComparison.Ordinal));

        var response = DeserializeHttpBinResponse(data.SOURCE);
        var expectedUri = new Uri(httpBin);
        var actualUri = new Uri(response.Url);
        Assert.Equal("value", response.Headers["Custom"]);
        Assert.True(response.Headers["Host"] == expectedUri.Host || response.Headers["Host"] == expectedUri.Authority);
        Assert.Equal("name1=value1; name2=value2", response.Headers["Cookie"]);
        Assert.Equal("GET", response.Method);
        Assert.Equal(expectedUri.Scheme, actualUri.Scheme);
        Assert.Equal(expectedUri.Host, actualUri.Host);
        Assert.Equal(expectedUri.AbsolutePath, actualUri.AbsolutePath);
    }

    [Theory]
    [InlineData(HttpLibrary.RuriLibHttp)]
    [InlineData(HttpLibrary.SystemNet)]
    [InlineData(HttpLibrary.CurlImpersonate)]
    public async Task HttpRequestStandard_Post_Verify(HttpLibrary library)
    {
        var httpBin = await TestHttpBin.BuildUrl("anything");
        var data = NewBotData();

        var options = new StandardHttpRequestOptions
        {
            Url = httpBin,
            Method = HttpMethod.POST,
            HttpLibrary = library,
            Content = "name1=value1&name2=value2",
            ContentType = "application/x-www-form-urlencoded"
        };

        await Methods.HttpRequestStandard(data, options);

        var response = DeserializeHttpBinResponse(data.SOURCE);
        Assert.Equal("POST", response.Method);
        Assert.Equal("value1", response.Form["name1"]);
        Assert.Equal("value2", response.Form["name2"]);
        Assert.Equal("application/x-www-form-urlencoded", response.Headers["Content-Type"]);
    }

    [Theory]
    [InlineData(HttpLibrary.RuriLibHttp)]
    [InlineData(HttpLibrary.SystemNet)]
    [InlineData(HttpLibrary.CurlImpersonate)]
    public async Task HttpRequestRaw_Post_Verify(HttpLibrary library)
    {
        var httpBin = await TestHttpBin.BuildUrl("anything");
        var data = NewBotData();

        var options = new RawHttpRequestOptions
        {
            Url = httpBin,
            Method = HttpMethod.POST,
            HttpLibrary = library,
            Content = Encoding.UTF8.GetBytes("name1=value1&name2=value2"),
            ContentType = "application/x-www-form-urlencoded"
        };

        await Methods.HttpRequestRaw(data, options);

        var response = DeserializeHttpBinResponse(data.SOURCE);
        Assert.Equal("POST", response.Method);
        Assert.Equal("value1", response.Form["name1"]);
        Assert.Equal("value2", response.Form["name2"]);
        Assert.Equal("application/x-www-form-urlencoded", response.Headers["Content-Type"]);
    }

    [Theory]
    [InlineData(HttpLibrary.RuriLibHttp)]
    [InlineData(HttpLibrary.SystemNet)]
    [InlineData(HttpLibrary.CurlImpersonate)]
    public async Task HttpRequestBasicAuth_Normal_Verify(HttpLibrary library)
    {
        var httpBin = await TestHttpBin.BuildUrl("anything");
        var data = NewBotData();

        var options = new BasicAuthHttpRequestOptions
        {
            Url = httpBin,
            Method = HttpMethod.GET,
            HttpLibrary = library,
            Username = "myUsername",
            Password = "myPassword"
        };

        await Methods.HttpRequestBasicAuth(data, options);

        var response = DeserializeHttpBinResponse(data.SOURCE);
        Assert.Equal("GET", response.Method);
        Assert.Equal(
            "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("myUsername:myPassword")),
            response.Headers["Authorization"]);
    }

    [Fact]
    public async Task HttpRequestStandard_HttpsToHttpRedirect_Verify()
    {
        await using var httpServer = LocalHttpResponseServer.CreateDelayed(
            TimeSpan.Zero,
            Encoding.UTF8.GetBytes("""{"redirected":true}"""),
            "Content-Type: application/json");
        await using var httpsServer = new LocalHttpsRedirectServer(
            LocalHttpsRedirectServer.CreateSelfSignedCertificate("localhost"),
            new Uri($"{httpServer.Uri}final"));
        var data = NewBotData();

        var options = new StandardHttpRequestOptions
        {
            Url = $"{httpsServer.Uri}start",
            Method = HttpMethod.GET,
            HttpLibrary = HttpLibrary.RuriLibHttp
        };

        await Methods.HttpRequestStandard(data, options);

        Assert.Equal(200, data.RESPONSECODE);
        Assert.Equal("""{"redirected":true}""", data.SOURCE);
        Assert.Equal($"{httpServer.Uri}final", data.ADDRESS);
    }

    [Fact]
    public async Task HttpRequestStandard_SystemNetSelfSignedCertificate_RespectsIgnoreCertificateValidation()
    {
        await using var httpServer = LocalHttpResponseServer.CreateDelayed(
            TimeSpan.Zero,
            Encoding.UTF8.GetBytes("""{"redirected":true}"""),
            "Content-Type: application/json");

        var failingData = NewBotData();

        await using (var failingHttpsServer = new LocalHttpsRedirectServer(
            LocalHttpsRedirectServer.CreateSelfSignedCertificate("localhost"),
            new Uri($"{httpServer.Uri}final")))
        {
            var failingOptions = new StandardHttpRequestOptions
            {
                Url = $"{failingHttpsServer.Uri}start",
                Method = HttpMethod.GET,
                HttpLibrary = HttpLibrary.SystemNet,
                IgnoreCertificateValidation = false
            };

            await Assert.ThrowsAsync<System.Net.Http.HttpRequestException>(
                () => Methods.HttpRequestStandard(failingData, failingOptions));
        }

        var passingData = NewBotData();

        await using (var passingHttpsServer = new LocalHttpsRedirectServer(
            LocalHttpsRedirectServer.CreateSelfSignedCertificate("localhost"),
            new Uri($"{httpServer.Uri}final")))
        {
            var passingOptions = new StandardHttpRequestOptions
            {
                Url = $"{passingHttpsServer.Uri}start",
                Method = HttpMethod.GET,
                HttpLibrary = HttpLibrary.SystemNet
            };

            await Methods.HttpRequestStandard(passingData, passingOptions);
        }

        Assert.Equal(302, passingData.RESPONSECODE);
    }

    [Theory]
    [InlineData(HttpLibrary.RuriLibHttp)]
    [InlineData(HttpLibrary.SystemNet)]
    [InlineData(HttpLibrary.CurlImpersonate)]
    public async Task HttpRequestMultipart_Post_Verify(HttpLibrary library)
    {
        var httpBin = await TestHttpBin.BuildUrl("anything");
        var data = NewBotData();

        var tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(tempFile, "fileContent");

            var contents = new List<MyHttpContent>
            {
                new StringHttpContent("stringName", "stringContent", "application/x-www-form-urlencoded"),
                new RawHttpContent("rawName", Encoding.UTF8.GetBytes("rawContent"), "application/octet-stream"),
                new FileHttpContent("fileName", tempFile, "application/octet-stream")
            };

            var options = new MultipartHttpRequestOptions
            {
                Url = httpBin,
                Method = HttpMethod.POST,
                HttpLibrary = library,
                Boundary = "myBoundary",
                Contents = contents
            };

            await Methods.HttpRequestMultipart(data, options);

            var response = DeserializeHttpBinResponse(data.SOURCE);
            Assert.Equal("POST", response.Method);
            Assert.Equal("stringContent", response.Form["stringName"]);
            Assert.Equal("rawContent", response.Form["rawName"]);
            Assert.Equal("fileContent", response.Files["fileName"]);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetAllCookies_ReturnsCookiesAcrossDomains()
    {
        var cookieJar = new CookieContainer();
        cookieJar.Add(new Uri("https://example.com/"), new Cookie("a", "1"));
        cookieJar.Add(new Uri("https://sub.example.com/test"), new Cookie("b", "2"));

        var cookies = global::RuriLib.Functions.Http.Http.GetAllCookies(cookieJar);

        Assert.Contains(cookies.Cast<Cookie>(), c => c.Name == "a" && c.Value == "1");
        Assert.Contains(cookies.Cast<Cookie>(), c => c.Name == "b" && c.Value == "2");
    }

    /*
    // Test for future implementation of HTTP/2.0
    [Fact]
    public async Task HttpRequestStandard_Http2_Verify()
    {
        var data = NewBotData();

        var options = new StandardHttpRequestOptions
        {
            Url = "https://http2.golang.org/reqinfo",
            Method = HttpMethod.GET,
            HttpVersion = "2.0"
        };

        await Methods.HttpRequestStandard(data, options);

        Assert.Contains("Protocol: HTTP/2.0", data.SOURCE);
    }
    */

    private static HttpBinResponse DeserializeHttpBinResponse(string source)
        => JsonConvert.DeserializeObject<HttpBinResponse>(source)
           ?? throw new InvalidOperationException("httpbin response could not be deserialized");
}
