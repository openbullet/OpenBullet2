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
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
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

    [Fact]
    public async Task HttpRequestStandard_Get_ThroughHttpsProxy_UsesTlsTransport_ForRuriLibHttp()
    {
        await using var proxyServer = await FakeHttpsProxyServer.StartAsync();
        var data = NewBotData(new Proxy("127.0.0.1", proxyServer.Port, ProxyType.Https));
        var options = new StandardHttpRequestOptions
        {
            Url = "http://example.com/test",
            Method = global::RuriLib.Functions.Http.HttpMethod.GET,
            HttpLibrary = HttpLibrary.RuriLibHttp,
            TimeoutMilliseconds = 5000,
            ReadResponseContent = true
        };

        await Methods.HttpRequestStandard(data, options);

        Assert.Equal(200, data.RESPONSECODE);
        Assert.Equal("OK", data.SOURCE);
        Assert.True(await proxyServer.WaitForTlsHandshakeAsync());
        Assert.StartsWith("GET http://example.com/test HTTP/", await proxyServer.WaitForFirstRequestLineAsync());
    }

    [Fact]
    public async Task HttpsProxyClient_ConnectStreamAsync_PreservesBytesAfterConnectResponse()
    {
        await using var proxyServer = await FakeHttpsProxyServer.StartAsync();
        var proxyClient = new HttpsProxyClient(new ProxySettings
        {
            Host = "127.0.0.1",
            Port = proxyServer.Port,
            ProxyCertificateValidationCallback = static (_, _, _, _) => true
        });

        await using var connection = await proxyClient.ConnectStreamAsync(
            "example.com",
            443,
            cancellationToken: TestContext.Current.CancellationToken);

        var prefixedBytes = new byte[4];
        await connection.Stream.ReadExactlyAsync(prefixedBytes, TestContext.Current.CancellationToken);

        var tunnelBytes = Encoding.ASCII.GetBytes("PING");
        await connection.Stream.WriteAsync(tunnelBytes, TestContext.Current.CancellationToken);
        await connection.Stream.FlushAsync(TestContext.Current.CancellationToken);

        Assert.Equal("PONG", Encoding.ASCII.GetString(prefixedBytes));
        Assert.True(await proxyServer.WaitForTlsHandshakeAsync());
        Assert.StartsWith("CONNECT example.com:443 HTTP/", await proxyServer.WaitForFirstRequestLineAsync());
        Assert.Equal("PING", await proxyServer.WaitForTunnelBytesAsync());
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

    private sealed class FakeHttpsProxyServer : IAsyncDisposable
    {
        private static readonly byte[] OkResponse = Encoding.ASCII.GetBytes(
            "HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\nContent-Type: text/plain\r\n\r\nOK");
        private static readonly byte[] ConnectEstablishedResponse = Encoding.ASCII.GetBytes(
            "HTTP/1.1 200 Connection Established\r\n\r\nPONG");

        private readonly TcpListener _listener;
        private readonly X509Certificate2 _certificate;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _acceptLoopTask;
        private readonly TaskCompletionSource<bool> _tlsHandshake = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<string> _firstRequestLine = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<string> _tunnelBytes = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        private FakeHttpsProxyServer(TcpListener listener, X509Certificate2 certificate)
        {
            _listener = listener;
            _certificate = certificate;
            _acceptLoopTask = AcceptLoopAsync();
        }

        public static Task<FakeHttpsProxyServer> StartAsync()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            return Task.FromResult(new FakeHttpsProxyServer(listener, CreateCertificate()));
        }

        public Task<bool> WaitForTlsHandshakeAsync()
            => _tlsHandshake.Task.WaitAsync(TestContext.Current.CancellationToken);

        public Task<string> WaitForFirstRequestLineAsync()
            => _firstRequestLine.Task.WaitAsync(TestContext.Current.CancellationToken);

        public Task<string> WaitForTunnelBytesAsync()
            => _tunnelBytes.Task.WaitAsync(TestContext.Current.CancellationToken);

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
                await using var sslStream = new SslStream(tcpClient.GetStream(), false);
                await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate = _certificate,
                    EnabledSslProtocols = SslProtocols.None,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }, _cts.Token).ConfigureAwait(false);

                _tlsHandshake.TrySetResult(true);
                var requestText = await ReadHeadersAsync(sslStream, _cts.Token).ConfigureAwait(false);
                var firstRequestLine = requestText.Split("\r\n", StringSplitOptions.None)[0];
                _firstRequestLine.TrySetResult(firstRequestLine);

                if (firstRequestLine.StartsWith("CONNECT ", StringComparison.OrdinalIgnoreCase))
                {
                    await sslStream.WriteAsync(
                        ConnectEstablishedResponse.AsMemory(0, ConnectEstablishedResponse.Length),
                        _cts.Token).ConfigureAwait(false);
                    await sslStream.FlushAsync(_cts.Token).ConfigureAwait(false);

                    var buffer = new byte[4];
                    var bytesRead = await sslStream.ReadAsync(buffer.AsMemory(0, buffer.Length), _cts.Token)
                        .ConfigureAwait(false);
                    _tunnelBytes.TrySetResult(Encoding.ASCII.GetString(buffer, 0, bytesRead));
                    return;
                }

                await sslStream.WriteAsync(OkResponse.AsMemory(0, OkResponse.Length), _cts.Token)
                    .ConfigureAwait(false);
                await sslStream.FlushAsync(_cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (AuthenticationException ex)
            {
                _tlsHandshake.TrySetException(ex);
            }
            catch (IOException ex)
            {
                _firstRequestLine.TrySetException(ex);
            }
            catch (Exception ex)
            {
                _tlsHandshake.TrySetException(ex);
                _firstRequestLine.TrySetException(ex);
                _tunnelBytes.TrySetException(ex);
            }
        }

        private static async Task<string> ReadHeadersAsync(Stream stream, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();
            var buffer = new byte[1024];

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)
                    .ConfigureAwait(false);

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

            return Encoding.ASCII.GetString(ms.ToArray());
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

        private static X509Certificate2 CreateCertificate()
        {
            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                "CN=localhost",
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
            request.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
            request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));

            using var certificate = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(1));

            return X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pfx), password: null);
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

            _certificate.Dispose();
            _cts.Dispose();
        }
    }
}

[CollectionDefinition(nameof(ProxyServerCollection), DisableParallelization = true)]
public class ProxyServerCollection;
