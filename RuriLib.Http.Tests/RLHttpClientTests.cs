using Newtonsoft.Json.Linq;
using RuriLib.Http.Models;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Http.Tests;

public class RLHttpClientTests
{
    [Fact]
    public async Task SendAsync_Get_Headers()
    {
        const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";

        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("http://httpbin.org/user-agent")
        };

        message.Headers.Add("User-Agent", userAgent);

        var response = await RequestAsync(message);
        var userAgentActual = await GetJsonValueAsync<string>(response, "user-agent");

        Assert.NotNull(userAgentActual);
        Assert.NotEmpty(userAgentActual);
        Assert.Equal(userAgent, userAgentActual);
    }

    [Fact]
    public async Task SendAsync_Get_Query()
    {
        const string key = "key";
        const string value = "value";

        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri($"http://httpbin.org/get?{key}={value}")
        };

        var response = await RequestAsync(message);
        var actual = await GetJsonDictionaryValueAsync(response, "args");

        Assert.NotNull(actual);
        Assert.True(actual.ContainsKey(key));
        Assert.True(actual.ContainsValue(value));
    }

    [Fact]
    public async Task SendAsync_Get_UTF8()
    {
        const string expected = "∮";

        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("http://httpbin.org/encoding/utf8")
        };

        var response = await RequestAsync(message);
        Assert.NotNull(response.Content);
        var actual = await response.Content.ReadAsStringAsync();

        Assert.Contains(expected, actual);
    }

    [Fact]
    public async Task SendAsync_Get_HTML()
    {
        const long expectedLength = 3741;
        const string contentType = "text/html";
        const string charSet = "utf-8";

        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("http://httpbin.org/html")
        };

        var response = await RequestAsync(message);

        var content = response.Content;
        Assert.NotNull(content);

        var headers = content.Headers;
        Assert.NotNull(headers);

        Assert.NotNull(headers.ContentLength);
        Assert.Equal(expectedLength, headers.ContentLength.Value);
        Assert.NotNull(headers.ContentType);
        Assert.Equal(contentType, headers.ContentType.MediaType);
        Assert.Equal(charSet, headers.ContentType.CharSet);
    }

    [Fact]
    public async Task SendAsync_Get_Delay()
    {
        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("http://httpbin.org/delay/4")
        };

        var response = await RequestAsync(message);
        Assert.NotNull(response.Content);
        var source = response.Content.ReadAsStringAsync();

        Assert.NotNull(response);
        Assert.NotNull(source);
    }

    [Fact]
    public async Task SendAsync_Get_Stream()
    {
        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("http://httpbin.org/stream/20")
        };

        var response = await RequestAsync(message);
        Assert.NotNull(response.Content);
        var source = response.Content.ReadAsStringAsync();

        Assert.NotNull(response);
        Assert.NotNull(source);
    }

    [Fact]
    public async Task SendAsync_Get_Gzip()
    {
        const string expected = "gzip, deflate";

        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("http://httpbin.org/gzip")
        };

        message.Headers["Accept-Encoding"] = expected;

        var response = await RequestAsync(message);
        var actual = await GetJsonDictionaryValueAsync(response, "headers");

        Assert.NotNull(actual);
        Assert.Equal(expected, actual["Accept-Encoding"]);
    }

    [Fact]
    public async Task SendAsync_Get_Cookies()
    {
        const string name = "name";
        const string value = "value";

        var cookies = new Dictionary<string, string>();

        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri($"http://httpbin.org/cookies/set?{name}={value}"),
            Cookies = cookies
        };

        var settings = new ProxySettings();
        var proxyClient = new NoProxyClient(settings);
        using var client = new RLHttpClient(proxyClient);
            
        await client.SendAsync(message);

        Assert.Single(cookies);
        Assert.Equal(value, cookies[name]);
    }

    [Fact]
    public async Task SendAsync_Get_StatusCode()
    {
        const string code = "404";
        const string expected = "NotFound";

        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri($"http://httpbin.org/status/{code}")
        };

        var response = await RequestAsync(message);

        Assert.NotNull(response);
        Assert.Equal(expected, response.StatusCode.ToString());
    }

    [Fact]
    public async Task SendAsync_Get_ExplicitHostHeader()
    {
        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("https://httpbin.org/headers")
        };
        message.Headers["Host"] = message.Uri.Host;

        var response = await RequestAsync(message);

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task SendAsync_GZip_Decompress()
    {
        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("https://nghttp2.org/httpbin/gzip")
        };

        var response = await RequestAsync(message);
        var actual = await GetJsonValueAsync<bool>(response, "gzipped");

        Assert.True(actual);
    }

    [Fact]
    public async Task SendAsync_Brotli_Decompress()
    {
        var message = new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri("https://nghttp2.org/httpbin/brotli")
        };

        var response = await RequestAsync(message);
        var actual = await GetJsonValueAsync<bool>(response, "brotli");

        Assert.True(actual);
    }

    private static async Task<HttpResponse> RequestAsync(HttpRequest request)
    {
        var settings = new ProxySettings();
        var proxyClient = new NoProxyClient(settings);
            
        using var client = new RLHttpClient(proxyClient);
        return await client.SendAsync(request);
    }

    private static async Task<T?> GetJsonValueAsync<T>(HttpResponse response, string valueName)
    {
        Assert.NotNull(response.Content);
        var source = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(source);

        var result = obj.TryGetValue(valueName, out var token);

        return result
            ? token!.Value<T>()
            : default;
    }

    private static async Task<Dictionary<string, string>?> GetJsonDictionaryValueAsync(HttpResponse response, string valueName)
    {
        Assert.NotNull(response.Content);
        var source = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(source);

        var result = obj.TryGetValue(valueName, out var token);

        return result
            ? token!.ToObject<Dictionary<string, string>>()
            : null;
    }
}
