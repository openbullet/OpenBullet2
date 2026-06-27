using RuriLib.Http.Curl;
using RuriLib.Http.Tests.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public async Task SendAsync_Http10Response_ReportsActualVersion()
    {
        await using var server = new CaptureHttpServer("ok", "HTTP/1.0");
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            UseBrowserHeaders = false,
            AllowAutoRedirect = false
        });
        using var client = new HttpClient(handler);

        using var response = await client.GetAsync(server.Uri, TestCancellationToken);

        Assert.Equal(new Version(1, 0), response.Version);
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
    public async Task SendAsync_WithoutBrowserHeaders_KeepsCommaSeparatedCustomHeaderOnSingleLine()
    {
        await using var server = new CaptureHttpServer("ok");
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            UseBrowserHeaders = false,
            AllowAutoRedirect = false
        });
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Uri);
        const string secChUa = "\"Not)A;Brand\";v=\"8\", \"Chromium\";v=\"138\", \"Google Chrome\";v=\"138\"";
        request.Headers.TryAddWithoutValidation("sec-ch-ua", secChUa);

        using var response = await client.SendAsync(request, TestCancellationToken);
        var rawRequest = await server.RawRequest;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"sec-ch-ua: {secChUa}\r\n", rawRequest);
        Assert.Equal(1, CountOccurrences(rawRequest, "\r\nsec-ch-ua: "));
    }

    [Fact]
    public async Task SendAsync_WithRequestHeadersCallback_ReportsActualHeaderOrder()
    {
        await using var server = new CaptureHttpServer("ok");
        var capturedRequests = new List<string>();
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            UseBrowserHeaders = false,
            AllowAutoRedirect = false,
            RequestHeadersCallback = capturedRequests.Add
        });
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, server.Uri);
        request.Headers.TryAddWithoutValidation("X-First", "one");
        request.Headers.TryAddWithoutValidation("X-Second", "two");

        using var response = await client.SendAsync(request, TestCancellationToken);

        var capturedRequest = Assert.Single(capturedRequests);
        Assert.Contains("GET / HTTP/1.1", capturedRequest);
        Assert.True(
            capturedRequest.IndexOf("X-First: one", StringComparison.Ordinal)
            < capturedRequest.IndexOf("X-Second: two", StringComparison.Ordinal));
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

    [Theory]
    [InlineData(CurlImpersonateBrowserProfile.Okhttp4Android10,
        "771,4865-4866-4867-49195-49196-52393-49199-49200-52392-49171-49172-156-157-47-53,23-65281-10-11-35-16-5-13-51-45-43,29-23-24,0")]
    [InlineData(CurlImpersonateBrowserProfile.SafariIos170,
        "771,4865-4866-4867-49196-49195-52393-49200-49199-52392-49162-49161-49172-49171-157-156-53-47-49160-49170-10,23-65281-10-11-16-5-13-18-51-45-43-27-21,29-23-24-25,0")]
    public async Task SendAsync_CustomProfiles_ProduceExpectedJa3Fingerprints(
        CurlImpersonateBrowserProfile profile, string expectedJa3)
    {
        var ja3 = Ja3.CalculateString(await CaptureClientHelloAsync(profile));

        Assert.Equal(expectedJa3, ja3);
    }

    [Fact]
    public async Task SendAsync_Http2Request_OffersH2InAlpn()
    {
        var protocols = await CaptureAlpnProtocolsAsync(CurlImpersonateBrowserProfile.Chrome142, HttpVersion.Version20);

        Assert.Contains("h2", protocols);
    }

    [Fact]
    public async Task SendAsync_CancellationWhileWaitingForResponse_CancelsPromptly()
    {
        await using var server = new HangingHttpServer();
        using var handler = new CurlImpersonateHandler(new CurlImpersonateHandlerOptions
        {
            UseBrowserHeaders = false,
            AllowAutoRedirect = false
        });
        using var client = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestCancellationToken);

        var requestTask = client.GetAsync(server.Uri, cts.Token);
        await server.RequestReceived.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);

        var sw = Stopwatch.StartNew();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await requestTask);
        sw.Stop();

        Assert.True(sw.Elapsed < TimeSpan.FromMilliseconds(500),
            $"Cancellation took {sw.ElapsedMilliseconds} ms");
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

    private static int CountOccurrences(string value, string pattern)
    {
        var count = 0;
        var index = 0;

        while ((index = value.IndexOf(pattern, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += pattern.Length;
        }

        return count;
    }

    private sealed class CaptureHttpServer : IAsyncDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource cts = new();
        private readonly Task acceptTask;
        private readonly string responseBody;
        private readonly string responseVersion;
        private readonly TaskCompletionSource<string> rawRequest =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public CaptureHttpServer(string responseBody, string responseVersion = "HTTP/1.1")
        {
            this.responseBody = responseBody;
            this.responseVersion = responseVersion;
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
                $"{responseVersion} 200 OK\r\n" +
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

    private sealed class HangingHttpServer : IAsyncDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource cts = new();
        private readonly Task acceptTask;
        private readonly TaskCompletionSource requestReceived =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public HangingHttpServer()
        {
            listener.Start();
            acceptTask = Task.Run(AcceptAsync);
        }

        public Uri Uri => new($"http://127.0.0.1:{((IPEndPoint)listener.LocalEndpoint).Port}/");

        public Task RequestReceived => requestReceived.Task;

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

            await ReadHeadersAsync(stream, linkedCts.Token);
            requestReceived.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token);
        }

        private static async Task ReadHeadersAsync(Stream stream, CancellationToken cancellationToken)
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
                    return;
                }
            }
        }
    }

}
