using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System;
using System.IO;
using RuriLib.Proxies.Clients;
using System.Threading;
using RuriLib.Proxies.Exceptions;

namespace RuriLib.Proxies.Tests
{
    public class ProxyClientTests
    {
        [Fact]
        public async Task ConnectAsync_NoProxyClient_Http()
        {
            var settings = new ProxySettings();
            var proxy = new NoProxyClient(settings);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var client = await proxy.ConnectAsync("example.com", 80, null, cts.Token);

            var response = await GetResponseAsync(client, BuildSampleGetRequest(), cts.Token);
            Assert.Contains("Example Domain", response);
        }

        /*
        [Fact]
        public async Task ConnectAsync_HttpProxyClient_Http()
        {
            var settings = new ProxySettings() { Host = "127.0.0.1", Port = 8888 };
            var proxy = new HttpProxyClient(settings);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var client = await proxy.ConnectAsync("example.com", 80, null, cts.Token);

            var response = await GetResponseAsync(client, BuildSampleGetRequest(), cts.Token);
            Assert.Contains("Example Domain", response);
        }
        */

        [Fact]
        public async Task ConnectAsync_HttpProxyClient_Invalid()
        {
            // Set an invalid proxy
            var settings = new ProxySettings() { Host = "example.com", Port = 80 };
            var proxy = new HttpProxyClient(settings);

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Assert.ThrowsAsync<ProxyException>(async () => await proxy.ConnectAsync("example.com", 80, null, cts.Token));
        }

        private static async Task<string> GetResponseAsync(TcpClient client, string request,
            CancellationToken cancellationToken = default)
        {
            using var netStream = client.GetStream();
            using var memory = new MemoryStream();

            // Send the data
            var requestBytes = Encoding.ASCII.GetBytes(request);
            await netStream.WriteAsync(requestBytes.AsMemory(0, requestBytes.Length), cancellationToken);

            // Read the response
            await netStream.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;
            var data = memory.ToArray();
            return Encoding.UTF8.GetString(data);
        }

        private static string BuildSampleGetRequest()
        {
            var requestLines = new string[]
            {
                "GET / HTTP/1.1",
                "Host: example.com",

                // We need this otherwise the server will default to Keep-Alive and
                // not close the stream leaving us hanging...
                "Connection: Close",
                "Accept: */*",
                string.Empty,
                string.Empty
            };

            return string.Join("\r\n", requestLines);
        }
    }
}
