using Newtonsoft.Json.Linq;
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
    public class ProxyClientHandlerTests
    {
        [Fact]
        public async Task SendAsync_Get_Headers()
        {
            var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.113 Safari/537.36";

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://httpbin.org/user-agent")
            };

            message.Headers.Add("User-Agent", userAgent);

            var response = await RequestAsync(message);
            var userAgentActual = await GetJsonStringValueAsync(response, "user-agent");

            Assert.NotEmpty(userAgentActual);
            Assert.Equal(userAgent, userAgentActual);
        }

        [Fact]
        public async Task SendAsync_Get_Query()
        {
            var key = "key";
            var value = "value";

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://httpbin.org/get?{key}={value}")
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

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://httpbin.org/encoding/utf8")
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

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://httpbin.org/html")
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
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://httpbin.org/delay/4")
            };

            var response = await RequestAsync(message);
            var source = response.Content.ReadAsStringAsync();

            Assert.NotNull(response);
            Assert.NotNull(source);
        }

        [Fact]
        public async Task SendAsync_Get_Stream()
        {
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://httpbin.org/stream/20")
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

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("http://httpbin.org/gzip")
            };

            message.Headers.TryAddWithoutValidation("Accept-Encoding", expected);

            var response = await RequestAsync(message);
            var actual = await GetJsonDictionaryValueAsync(response, "headers");

            Assert.Equal(expected, actual["Accept-Encoding"]);
        }

        [Fact]
        public async Task SendAsync_Get_Cookies()
        {
            var name = "name";
            var value = "value";

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://httpbin.org/cookies/set?{name}={value}")
            };

            var settings = new ProxySettings();
            var proxyClient = new NoProxyClient(settings);
            var cookieContainer = new CookieContainer();
            using var proxyClientHandler = new ProxyClientHandler(proxyClient)
            {
                CookieContainer = cookieContainer
            };

            using var client = new HttpClient(proxyClientHandler);
            var response = await client.SendAsync(message);

            var cookies = cookieContainer.GetCookies(new Uri("http://httpbin.org/"));

            Assert.Single(cookies);
            var cookie = cookies[name];
            Assert.Equal(name, cookie.Name);
            Assert.Equal(value, cookie.Value);

            client.Dispose();
        }

        [Fact]
        public async Task SendAsync_Get_StatusCode()
        {
            var code = "404";
            var expected = "NotFound";

            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"http://httpbin.org/status/{code}")
            };

            var response = await RequestAsync(message);

            Assert.NotNull(response);
            Assert.Equal(expected, response.StatusCode.ToString());
        }

        [Fact]
        public async Task SendAsync_Get_ExplicitHostHeader()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, "https://httpbin.org/headers");
            message.Headers.Host = "httpbin.org";

            var response = await RequestAsync(message);

            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static async Task<HttpResponseMessage> RequestAsync(HttpRequestMessage request)
        {
            var settings = new ProxySettings();
            var proxyClient = new NoProxyClient(settings);
            using var proxyClientHandler = new ProxyClientHandler(proxyClient)
            {
                CookieContainer = new CookieContainer()
            };

            using var client = new HttpClient(proxyClientHandler);
            return await client.SendAsync(request);
        }

        private static async Task<string> GetJsonStringValueAsync(HttpResponseMessage response, string valueName)
        {
            var source = await response.Content.ReadAsStringAsync();
            var obj = JObject.Parse(source);

            var result = obj.TryGetValue(valueName, out var token);

            return result
                ? token.Value<string>()
                : string.Empty;
        }

        private static async Task<Dictionary<string, string>> GetJsonDictionaryValueAsync(HttpResponseMessage response, string valueName)
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
