using RuriLib.Http.Curl;
using RuriLib.Http.Tests.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Http.Tests;

public class CurlImpersonateHandlerTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task SendAsync_Get_ReturnsResponse()
    {
        await using var server = new CaptureHttpServer("hello from curl");
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            UseBrowserHeaders = false,
            AllowAutoRedirect = false
        });
        using var client = new HttpClient(handler);

        using var response = await client.GetAsync(server.Uri, TestCancellationToken);
        var body = await response.Content.ReadAsStringAsync(TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("hello from curl", body);
    }

    [Fact]
    public async Task SendAsync_WithBrowserHeaders_DoesNotOverrideBrowserManagedHeaders()
    {
        await using var server = new CaptureHttpServer("ok");
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            BrowserProfile = CurlImpersonateBrowserProfile.Chrome142,
            UseBrowserHeaders = true,
            AllowAutoRedirect = false
        });
        using var client = new HttpClient(handler);

        using var request = new HttpRequestMessage(HttpMethod.Get, server.Uri);
        request.Headers.TryAddWithoutValidation("User-Agent", "BadAgent/1.0");
        request.Headers.TryAddWithoutValidation("X-Custom", "kept");

        using var response = await client.SendAsync(request, TestCancellationToken);
        var rawRequest = await server.RawRequest;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("BadAgent/1.0", rawRequest);
        Assert.Contains("X-Custom: kept", rawRequest);
    }

    [Fact]
    public async Task SendAsync_DifferentProfiles_ProduceDifferentJa3Fingerprints()
    {
        var chromeJa3 = await CaptureJa3Async(CurlImpersonateBrowserProfile.Chrome142);
        var firefoxJa3 = await CaptureJa3Async(CurlImpersonateBrowserProfile.Firefox144);

        Assert.False(string.IsNullOrWhiteSpace(chromeJa3));
        Assert.False(string.IsNullOrWhiteSpace(firefoxJa3));
        Assert.NotEqual(chromeJa3, firefoxJa3);
    }

    [Fact]
    public async Task SendAsync_Http2Request_OffersH2InAlpn()
    {
        var protocols = await CaptureAlpnProtocolsAsync(CurlImpersonateBrowserProfile.Chrome142, HttpVersion.Version20);

        Assert.Contains("h2", protocols);
    }

    private static async Task<string> CaptureJa3Async(CurlImpersonateBrowserProfile profile)
        => Ja3.CalculateHash(await CaptureClientHelloAsync(profile));

    private static async Task<IReadOnlyList<string>> CaptureAlpnProtocolsAsync(CurlImpersonateBrowserProfile profile,
        Version httpVersion)
        => TlsClientHello.GetAlpnProtocols(await CaptureClientHelloAsync(profile, request =>
        {
            request.Version = httpVersion;
        }));

    private static async Task<byte[]> CaptureClientHelloAsync(CurlImpersonateBrowserProfile profile,
        Action<HttpRequestMessage>? configureRequest = null)
    {
        await using var server = new TlsClientHelloCaptureServer(TestCancellationToken);
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            BrowserProfile = profile,
            IgnoreCertificateValidation = true,
            ConnectTimeout = TimeSpan.FromSeconds(2),
            Timeout = TimeSpan.FromSeconds(5),
            AllowAutoRedirect = false
        });
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Uri);
        configureRequest?.Invoke(request);

        var requestTask = client.SendAsync(request, TestCancellationToken);
        var clientHelloTask = server.ClientHello;

        try
        {
            await requestTask;
        }
        catch
        {
            // The test server intentionally captures ClientHello and closes the TCP connection.
        }

        return await clientHelloTask.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
    }

    private sealed class CaptureHttpServer : IAsyncDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource cts = new();
        private readonly Task acceptTask;
        private readonly string responseBody;
        private readonly TaskCompletionSource<string> rawRequest =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CaptureHttpServer(string responseBody)
        {
            this.responseBody = responseBody;
            listener.Start();
            acceptTask = Task.Run(AcceptAsync);
        }

        public Uri Uri => new($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/");

        public Task<string> RawRequest => rawRequest.Task;

        public async ValueTask DisposeAsync()
        {
            await cts.CancelAsync();
            listener.Stop();

            try
            {
                await acceptTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            finally
            {
                cts.Dispose();
            }
        }

        private async Task AcceptAsync()
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, TestCancellationToken);
            using var client = await listener.AcceptTcpClientAsync(linkedCts.Token);
            await using var stream = client.GetStream();

            var request = await ReadHeadersAsync(stream, linkedCts.Token);
            rawRequest.TrySetResult(request);

            var body = Encoding.UTF8.GetBytes(responseBody);
            var headers = Encoding.ASCII.GetBytes(
                "HTTP/1.1 200 OK\r\n" +
                $"Content-Length: {body.Length}\r\n" +
                "Connection: close\r\n" +
                "\r\n");

            await stream.WriteAsync(headers, linkedCts.Token);
            await stream.WriteAsync(body, linkedCts.Token);
        }

        private static async Task<string> ReadHeadersAsync(Stream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var ms = new MemoryStream();

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);

                if (bytesRead == 0)
                {
                    throw new InvalidOperationException("Client closed the connection before sending headers");
                }

                ms.Write(buffer, 0, bytesRead);
                var request = Encoding.ASCII.GetString(ms.ToArray());

                if (request.Contains("\r\n\r\n", StringComparison.Ordinal))
                {
                    return request;
                }
            }
        }
    }

}
