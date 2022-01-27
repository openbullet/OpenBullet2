using RuriLib.Functions.Http;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using RuriLib.Blocks.Requests.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuriLib.Tests.Utils;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using System.IO;
using RuriLib.Tests.Utils.Mockup;
using RuriLib.Functions.Http.Options;

namespace RuriLib.Tests.Functions.Http
{
    public class HttpTests
    {
        private static BotData NewBotData() => new(
            new(null) 
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("", new WordlistType()),
            null,
            false);

        private readonly string httpBin = "https://httpbin.org/anything";

        [Theory]
        [InlineData(HttpLibrary.RuriLibHttp)]
        [InlineData(HttpLibrary.SystemNet)]
        public async Task HttpRequestStandard_Get_Verify(HttpLibrary library)
        {
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

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("value", response.Headers["Custom"]);
            Assert.Equal("httpbin.org", response.Headers["Host"]);
            Assert.Equal("name1=value1; name2=value2", response.Headers["Cookie"]);
            Assert.Equal("GET", response.Method);
            Assert.Equal(httpBin, response.Url);
        }

        [Theory]
        [InlineData(HttpLibrary.RuriLibHttp)]
        [InlineData(HttpLibrary.SystemNet)]
        public async Task HttpRequestStandard_Post_Verify(HttpLibrary library)
        {
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

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("POST", response.Method);
            Assert.Equal("value1", response.Form["name1"]);
            Assert.Equal("value2", response.Form["name2"]);
            Assert.Equal("application/x-www-form-urlencoded", response.Headers["Content-Type"]);
        }

        [Theory]
        [InlineData(HttpLibrary.RuriLibHttp)]
        [InlineData(HttpLibrary.SystemNet)]
        public async Task HttpRequestRaw_Post_Verify(HttpLibrary library)
        {
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

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("POST", response.Method);
            Assert.Equal("value1", response.Form["name1"]);
            Assert.Equal("value2", response.Form["name2"]);
            Assert.Equal("application/x-www-form-urlencoded", response.Headers["Content-Type"]);
        }

        [Theory]
        [InlineData(HttpLibrary.RuriLibHttp)]
        [InlineData(HttpLibrary.SystemNet)]
        public async Task HttpRequestBasicAuth_Normal_Verify(HttpLibrary library)
        {
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

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("GET", response.Method);
            Assert.Equal("Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes("myUsername:myPassword")), response.Headers["Authorization"]);
        }

        [Theory]
        [InlineData(HttpLibrary.RuriLibHttp)]
        [InlineData(HttpLibrary.SystemNet)]
        public async Task HttpRequestMultipart_Post_Verify(HttpLibrary library)
        {
            var data = NewBotData();

            var tempFile = Path.GetTempFileName();
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

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("POST", response.Method);
            Assert.Equal("stringContent", response.Form["stringName"]);
            Assert.Equal("rawContent", response.Form["rawName"]);
            Assert.Equal("fileContent", response.Files["fileName"]);
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
    }
}
