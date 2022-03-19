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

namespace RuriLib.Http.Tests
{
    public class RLHttpClientTests
    {
        [Fact]
        public async Task SendAsync_Get_Headers()
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";

            var message = new HttpRequest
            {
                Method = HttpMethod.Get,
                Uri = new Uri("http://httpbin.org/user-agent")
            };

            message.Headers.Add("User-Agent", userAgent);

            var response = await RequestAsync(message);
            var userAgentActual = await GetJsonValueAsync<string>(response, "user-agent");

            Assert.NotEmpty(userAgentActual);
            Assert.Equal(userAgent, userAgentActual);
        }

        [Fact]
        public async Task SendAsync_Get_Query()
        {
            var key = "key";
            var value = "value";

            var message = new HttpRequest
            {
                Method = HttpMethod.Get,
                Uri = new Uri($"http://httpbin.org/get?{key}={value}")
            };

            var response = await RequestAsync(message);
            var actual = await GetJsonDictionaryValueAsync(response, "args");

            Assert.True(actual.ContainsKey(key));
            Assert.True(actual.ContainsValue(value));
        }

        [Fact]
        public async Task SendAsync_Get_UTF8()
        {
            var expected = "∮";

            var message = new HttpRequest
            {
                Method = HttpMethod.Get,
                Uri = new Uri("http://httpbin.org/encoding/utf8")
            };

            var response = await RequestAsync(message);
            var actual = await response.Content.ReadAsStringAsync();

            Assert.Contains(expected, actual);
        }

        [Fact]
        public async Task SendAsync_Get_HTML()
        {
            long expectedLength = 3741;
            var contentType = "text/html";
            var charSet = "utf-8";

            var message = new HttpRequest
            {
                Method = HttpMethod.Get,
                Uri = new Uri("http://httpbin.org/html")
            };

            var response = await RequestAsync(message);

            var content = response.Content;
            Assert.NotNull(content);

            var headers = response.Content.Headers;
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
            var source = response.Content.ReadAsStringAsync();

            Assert.NotNull(response);
            Assert.NotNull(source);
        }

        [Fact]
        public async Task SendAsync_Get_Gzip()
        {
            var expected = "gzip, deflate";

            var message = new HttpRequest
            {
                Method = HttpMethod.Get,
                Uri = new Uri("http://httpbin.org/gzip")
            };

            message.Headers["Accept-Encoding"] = expected;

            var response = await RequestAsync(message);
            var actual = await GetJsonDictionaryValueAsync(response, "headers");

            Assert.Equal(expected, actual["Accept-Encoding"]);
        }

        [Fact]
        public async Task SendAsync_Get_Cookies()
        {
            var name = "name";
            var value = "value";

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
            
            var response = await client.SendAsync(message);

            Assert.Single(cookies);
            Assert.Equal(value, cookies[name]);
        }

        [Fact]
        public async Task SendAsync_Get_StatusCode()
        {
            var code = "404";
            var expected = "NotFound";

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
            message.Headers["Host"] = "httpbin.org";

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

        private static async Task<T> GetJsonValueAsync<T>(HttpResponse response, string valueName)
        {
            var source = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(source);

            var result = obj.TryGetValue(valueName, out var token);

            return result
                ? token.Value<T>()
                : default;
        }

        private static async Task<Dictionary<string, string>> GetJsonDictionaryValueAsync(HttpResponse response, string valueName)
        {
            var source = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(source);

            var result = obj.TryGetValue(valueName, out var token);

            return result
                ? token.ToObject<Dictionary<string, string>>()
                : null;
        }
    }
}
