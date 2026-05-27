using Newtonsoft.Json;
using RuriLib.Blocks.Requests.Http;
using RuriLib.Http;
using RuriLib.Functions.Http;
using RuriLib.Functions.Http.Options;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using RuriLib.Tests.Utils;
using RuriLib.Tests.Utils.Mockup;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Http;

[Collection(nameof(ProxyServerCollection))]
public class HttpRequestProxyIntegrationTests
{
    [Theory]
    [InlineData(HttpLibrary.RuriLibHttp, ProxyType.Http)]
    [InlineData(HttpLibrary.RuriLibHttp, ProxyType.Socks4)]
    [InlineData(HttpLibrary.RuriLibHttp, ProxyType.Socks4a)]
    [InlineData(HttpLibrary.RuriLibHttp, ProxyType.Socks5)]
    [InlineData(HttpLibrary.SystemNet, ProxyType.Http)]
    [InlineData(HttpLibrary.SystemNet, ProxyType.Socks4)]
    [InlineData(HttpLibrary.SystemNet, ProxyType.Socks4a)]
    [InlineData(HttpLibrary.SystemNet, ProxyType.Socks5)]
    [InlineData(HttpLibrary.CurlImpersonate, ProxyType.Http)]
    [InlineData(HttpLibrary.CurlImpersonate, ProxyType.Socks5)]
    public async Task HttpRequestStandard_Get_ThroughProxy_Verify(HttpLibrary library, ProxyType proxyType)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var proxy = connection.CreateProxy(proxyType);
        var data = NewBotData(proxy);

        var queryValue = $"{library}-{proxyType}".ToLowerInvariant();
        var options = new StandardHttpRequestOptions
        {
            Url = connection.BuildTargetUrl($"anything?proxy={queryValue}"),
            Method = global::RuriLib.Functions.Http.HttpMethod.GET,
            HttpLibrary = library,
            TimeoutMilliseconds = 20000,
            CustomHeaders =
            {
                ["Custom-Proxy-Test"] = queryValue
            }
        };

        await Methods.HttpRequestStandard(data, options);

        var response = DeserializeHttpBinResponse(data.SOURCE);
        var actualUri = new Uri(response.Url);

        Assert.Equal("GET", response.Method);
        Assert.Equal(queryValue, response.Headers["Custom-Proxy-Test"]);
        Assert.Equal("/anything", actualUri.AbsolutePath);
        Assert.Equal($"?proxy={queryValue}", actualUri.Query);
        Assert.Equal(200, data.RESPONSECODE);
    }

    [Theory]
    [InlineData(HttpLibrary.RuriLibHttp, ProxyType.Http)]
    [InlineData(HttpLibrary.RuriLibHttp, ProxyType.Socks5)]
    [InlineData(HttpLibrary.SystemNet, ProxyType.Http)]
    [InlineData(HttpLibrary.SystemNet, ProxyType.Socks5)]
    [InlineData(HttpLibrary.CurlImpersonate, ProxyType.Http)]
    [InlineData(HttpLibrary.CurlImpersonate, ProxyType.Socks5)]
    public async Task HttpRequestStandard_Get_ThroughAuthenticatedProxy_Verify(HttpLibrary library, ProxyType proxyType)
    {
        var connection = await TestProxyServer.GetConnectionInfo();
        var proxy = connection.CreateAuthenticatedProxy(proxyType);
        var data = NewBotData(proxy);

        var queryValue = $"{library}-{proxyType}-auth".ToLowerInvariant();
        var options = new StandardHttpRequestOptions
        {
            Url = connection.BuildTargetUrl($"anything?proxy={queryValue}"),
            Method = global::RuriLib.Functions.Http.HttpMethod.GET,
            HttpLibrary = library,
            TimeoutMilliseconds = 20000,
            CustomHeaders =
            {
                ["Custom-Proxy-Test"] = queryValue
            }
        };

        await Methods.HttpRequestStandard(data, options);

        var response = DeserializeHttpBinResponse(data.SOURCE);
        var actualUri = new Uri(response.Url);

        Assert.Equal("GET", response.Method);
        Assert.Equal(queryValue, response.Headers["Custom-Proxy-Test"]);
        Assert.Equal("/anything", actualUri.AbsolutePath);
        Assert.Equal($"?proxy={queryValue}", actualUri.Query);
        Assert.Equal(200, data.RESPONSECODE);
    }

    [Fact]
    public async Task HttpRequestStandard_Get_ThroughHttpProxy_WhenProxyReturnsBadGateway_ReturnsBadGateway_ForSystemNet()
    {
        await using var proxyServer = await FakeBadGatewayProxyServer.StartAsync();
        var data = NewBotData(new Proxy("127.0.0.1", proxyServer.Port, ProxyType.Http));
        var options = NewBadGatewayRequestOptions(HttpLibrary.SystemNet);

        await Methods.HttpRequestStandard(data, options);

        Assert.Equal(502, data.RESPONSECODE);
        Assert.Equal("Bad Gateway", data.SOURCE);
        Assert.StartsWith("GET http://example.com/test HTTP/", await proxyServer.WaitForFirstRequestLineAsync());
    }

    [Fact]
    public async Task HttpRequestStandard_Get_ThroughHttpProxy_WhenProxyReturnsBadGateway_ReturnsBadGateway_ForRuriLibHttp()
    {
        await using var proxyServer = await FakeBadGatewayProxyServer.StartAsync();
        var data = NewBotData(new Proxy("127.0.0.1", proxyServer.Port, ProxyType.Http));
        var options = NewBadGatewayRequestOptions(HttpLibrary.RuriLibHttp);

        await Methods.HttpRequestStandard(data, options);

        Assert.Equal(502, data.RESPONSECODE);
        Assert.Equal("Bad Gateway", data.SOURCE);
        Assert.StartsWith("GET http://example.com/test HTTP/", await proxyServer.WaitForFirstRequestLineAsync());
    }

    [Fact]
    public async Task ProxyClientHandler_Get_ThroughHttpProxy_WhenProxyReturnsBadGateway_ReturnsBadGateway()
    {
        await using var proxyServer = await FakeBadGatewayProxyServer.StartAsync();
        using var handler = new ProxyClientHandler(new HttpProxyClient(new ProxySettings
        {
            Host = "127.0.0.1",
            Port = proxyServer.Port
        }));
        using var client = new HttpClient(handler);
        using var response = await client.GetAsync("http://example.com/test", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal("Bad Gateway", await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.StartsWith("GET http://example.com/test HTTP/", await proxyServer.WaitForFirstRequestLineAsync());
    }

    private static BotData NewBotData(Proxy proxy)
        => new(
            new global::RuriLib.Models.Bots.Providers(null!)
            {
                ProxySettings = new MockedProxySettingsProvider(),
                Security = new MockedSecurityProvider()
            },
            new ConfigSettings(),
            new BotLogger(),
            new DataLine("proxy-http-test", new WordlistType()),
            proxy,
            useProxy: true)
        {
            CancellationToken = TestContext.Current.CancellationToken
        };

    private static HttpBinResponse DeserializeHttpBinResponse(string source)
        => JsonConvert.DeserializeObject<HttpBinResponse>(source)
           ?? throw new InvalidOperationException("httpbin response could not be deserialized");

    private static StandardHttpRequestOptions NewBadGatewayRequestOptions(HttpLibrary library)
        => new()
        {
            Url = "http://example.com/test",
            Method = global::RuriLib.Functions.Http.HttpMethod.GET,
            HttpLibrary = library,
            TimeoutMilliseconds = 5000,
            ReadResponseContent = true
        };

    private sealed class FakeBadGatewayProxyServer : IAsyncDisposable
    {
        private const string BadGatewayBody = "Bad Gateway";
        private static readonly byte[] BadGatewayResponse = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 502 Bad Gateway\r\nContent-Length: {BadGatewayBody.Length}\r\nConnection: close\r\nContent-Type: text/plain\r\n\r\n{BadGatewayBody}");

        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _acceptLoopTask;
        private readonly TaskCompletionSource<string> _firstRequestLine = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        private FakeBadGatewayProxyServer(TcpListener listener)
        {
            _listener = listener;
            _acceptLoopTask = AcceptLoopAsync();
        }

        public static Task<FakeBadGatewayProxyServer> StartAsync()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return Task.FromResult(new FakeBadGatewayProxyServer(listener));
        }

        public Task<string> WaitForFirstRequestLineAsync()
            => _firstRequestLine.Task.WaitAsync(TestContext.Current.CancellationToken);

        private async Task AcceptLoopAsync()
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    var client = await _listener.AcceptTcpClientAsync(_cts.Token).ConfigureAwait(false);
                    _ = HandleClientAsync(client);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using var tcpClient = client;

            try
            {
                using var stream = tcpClient.GetStream();
                using var ms = new MemoryStream();
                var buffer = new byte[1024];

                while (true)
                {
                    var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), _cts.Token).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    ms.Write(buffer, 0, bytesRead);

                    if (HasReachedEndOfHeaders(ms))
                    {
                        break;
                    }
                }

                var requestText = Encoding.ASCII.GetString(ms.ToArray());
                var firstRequestLine = requestText.Split("\r\n", StringSplitOptions.None)[0];
                _firstRequestLine.TrySetResult(firstRequestLine);

                await stream.WriteAsync(BadGatewayResponse.AsMemory(0, BadGatewayResponse.Length), _cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private static bool HasReachedEndOfHeaders(MemoryStream ms)
        {
            if (ms.Length < 4)
            {
                return false;
            }

            var buffer = ms.GetBuffer();
            var length = (int)ms.Length;

            return buffer[length - 4] == '\r'
                && buffer[length - 3] == '\n'
                && buffer[length - 2] == '\r'
                && buffer[length - 1] == '\n';
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await _cts.CancelAsync().ConfigureAwait(false);
            }
            catch
            {
            }

            _listener.Stop();

            try
            {
                await _acceptLoopTask.ConfigureAwait(false);
            }
            catch
            {
            }

            _cts.Dispose();
        }
    }
}

[CollectionDefinition(nameof(ProxyServerCollection), DisableParallelization = true)]
public class ProxyServerCollection;
