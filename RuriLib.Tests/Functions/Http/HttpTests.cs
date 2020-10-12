using RuriLib.Functions.Http;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Settings;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;
using RuriLib.Blocks.Requests.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RuriLib.Tests.HttpBin;
using RuriLib.Models.Blocks.Custom.HttpRequest.Multipart;
using System.IO;

namespace RuriLib.Tests.Functions.Http
{
    public class HttpTests
    {
        private BotData NewData() => new BotData(
            new GlobalSettings(),
            new ConfigSettings(),
            new BotLogger(),
            null,
            new Random(),
            new DataLine("", new WordlistType()),
            null,
            false);

        private readonly string httpBin = "https://httpbin.org/anything";
        private readonly int timeout = 5000;

        [Fact]
        public async Task HttpRequestStandard_Get_Verify()
        {
            var data = NewData();

            var cookies = new Dictionary<string, string>
            {
                { "name1", "value1" },
                { "name2", "value2" }
            };

            var headers = new Dictionary<string, string>
            {
                { "Custom", "value" }
            };

            await Methods.HttpRequestStandard(data, httpBin, HttpMethod.GET, true, SecurityProtocol.SystemDefault,
                "", "", cookies, headers, timeout, "1.1");

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("value", response.Headers["Custom"]);
            Assert.Equal("httpbin.org", response.Headers["Host"]);
            Assert.Equal("name1=value1; name2=value2", response.Headers["Cookie"]);
            Assert.Equal("GET", response.Method);
            Assert.Equal(httpBin, response.Url);
        }

        [Fact]
        public async Task HttpRequestStandard_Post_Verify()
        {
            var data = NewData();

            await Methods.HttpRequestStandard(data, httpBin, HttpMethod.POST, true, SecurityProtocol.SystemDefault,
                "name1=value1&name2=value2", "application/x-www-form-urlencoded",
                new Dictionary<string, string>(), new Dictionary<string, string>(), timeout, "1.1");

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("POST", response.Method);
            Assert.Equal("value1", response.Form["name1"]);
            Assert.Equal("value2", response.Form["name2"]);
            Assert.Equal("application/x-www-form-urlencoded; charset=utf-8", response.Headers["Content-Type"]);
        }

        [Fact]
        public async Task HttpRequestRaw_Post_Verify()
        {
            var data = NewData();

            await Methods.HttpRequestRaw(data, httpBin, HttpMethod.POST, true, SecurityProtocol.SystemDefault,
                Encoding.UTF8.GetBytes("name1=value1&name2=value2"), "application/x-www-form-urlencoded",
                new Dictionary<string, string>(), new Dictionary<string, string>(), timeout, "1.1");

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("POST", response.Method);
            Assert.Equal("value1", response.Form["name1"]);
            Assert.Equal("value2", response.Form["name2"]);
            Assert.Equal("application/x-www-form-urlencoded", response.Headers["Content-Type"]);
        }

        [Fact]
        public async Task HttpRequestBasicAuth_Normal_Verify()
        {
            var data = NewData();

            await Methods.HttpRequestBasicAuth(data, httpBin, true, SecurityProtocol.SystemDefault,
                "myUsername", "myPassword",
                new Dictionary<string, string>(), new Dictionary<string, string>(), timeout, "1.1");

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("GET", response.Method);
            Assert.Equal(Convert.ToBase64String(Encoding.UTF8.GetBytes("myUsername:myPassword")), response.Headers["Authorization"]);
        }

        [Fact]
        public async Task HttpRequestMultipart_Post_Verify()
        {
            var data = NewData();

            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, "fileContent");

            var contents = new List<MyHttpContent>
            {
                new StringHttpContent("stringName", "stringContent", "application/x-www-form-urlencoded"),
                new RawHttpContent("rawName", Encoding.UTF8.GetBytes("rawContent"), "application/octet-stream"),
                new FileHttpContent("fileName", tempFile, "application/octet-stream")
            };

            await Methods.HttpRequestMultipart(data, httpBin, HttpMethod.POST, true, SecurityProtocol.SystemDefault,
                "myBoundary", contents,
                new Dictionary<string, string>(), new Dictionary<string, string>(), timeout, "1.1");

            var response = JsonConvert.DeserializeObject<HttpBinResponse>(data.SOURCE);
            Assert.Equal("POST", response.Method);
            Assert.Equal("stringContent", response.Form["stringName"]);
            Assert.Equal("rawContent", response.Form["rawName"]);
            Assert.Equal("fileContent", response.Files["fileName"]);
        }

        [Fact]
        public async Task HttpRequestStandard_Http2_Verify()
        {
            var data = NewData();

            await Methods.HttpRequestStandard(data, "https://http2.golang.org/reqinfo", HttpMethod.GET, true, SecurityProtocol.SystemDefault,
                "", "", new Dictionary<string, string>(), new Dictionary<string, string>(), timeout, "2.0");

            Assert.Contains("Protocol: HTTP/2.0", data.SOURCE);
        }
    }
}
