using RuriLib.Http.Models;
using RuriLib.Proxies.Clients;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Http.Tests;

public class RedirectTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task RLHttpClient_HttpsToHttpRedirect_FollowsRedirect()
    {
        await using var httpServer = LocalHttpTestServer.CreateHttpOk("redirect-ok");
        using var certificate = LocalHttpTestServer.CreateSelfSignedCertificate("localhost");
        await using var httpsServer = LocalHttpTestServer.CreateHttpsRedirect(
            certificate,
            new Uri($"http://localhost:{httpServer.Port}/final"));

        using var client = new RLHttpClient(new NoProxyClient());

        using var response = await client.SendAsync(new HttpRequest
        {
            Method = HttpMethod.Get,
            Uri = new Uri($"https://localhost:{httpsServer.Port}/start")
        }, TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(response.Content);
        Assert.Equal("redirect-ok", await response.Content.ReadAsStringAsync(TestCancellationToken));
        Assert.Equal($"http://localhost:{httpServer.Port}/final", response.Request?.Uri?.AbsoluteUri);
    }

    [Fact]
    public async Task ProxyClientHandler_HttpsToHttpRedirect_FollowsRedirect()
    {
        await using var httpServer = LocalHttpTestServer.CreateHttpOk("redirect-ok");
        using var certificate = LocalHttpTestServer.CreateSelfSignedCertificate("localhost");
        await using var httpsServer = LocalHttpTestServer.CreateHttpsRedirect(
            certificate,
            new Uri($"http://localhost:{httpServer.Port}/final"));

        using var handler = new ProxyClientHandler(new NoProxyClient());
        using var client = new HttpClient(handler);
        using var response = await client.GetAsync(
            $"https://localhost:{httpsServer.Port}/start",
            TestCancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("redirect-ok", await response.Content.ReadAsStringAsync(TestCancellationToken));
        Assert.Equal($"http://localhost:{httpServer.Port}/final", response.RequestMessage?.RequestUri?.AbsoluteUri);
    }

    private sealed class LocalHttpTestServer : IAsyncDisposable
    {
        private readonly TcpListener _listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly Func<TcpClient, CancellationToken, Task> _handler;
        private readonly Task _acceptTask;

        private LocalHttpTestServer(Func<TcpClient, CancellationToken, Task> handler)
        {
            _handler = handler;
            _listener.Start();
            _acceptTask = Task.Run(AcceptClientAsync, CancellationToken.None);
        }

        public int Port => ((IPEndPoint)_listener.LocalEndpoint).Port;

        public static LocalHttpTestServer CreateHttpOk(string responseBody)
        {
            ArgumentNullException.ThrowIfNull(responseBody);

            return new(async (client, cancellationToken) =>
            {
                await using var stream = client.GetStream();
                await ReadHeadersAsync(stream, cancellationToken);

                var payload = Encoding.UTF8.GetBytes(responseBody);
                var response =
                    $"HTTP/1.1 200 OK\r\nContent-Length: {payload.Length}\r\nConnection: close\r\n\r\n";

                await stream.WriteAsync(Encoding.ASCII.GetBytes(response), cancellationToken);
                await stream.WriteAsync(payload, cancellationToken);
            });
        }

        public static LocalHttpTestServer CreateHttpsRedirect(X509Certificate2 certificate, Uri redirectUri)
        {
            ArgumentNullException.ThrowIfNull(certificate);
            ArgumentNullException.ThrowIfNull(redirectUri);

            return new(async (client, cancellationToken) =>
            {
                await using var networkStream = client.GetStream();
                await using var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false);

                await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate = certificate,
                    ClientCertificateRequired = false,
                    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
                }, cancellationToken);

                await ReadHeadersAsync(sslStream, cancellationToken);

                var response =
                    $"HTTP/1.1 302 Found\r\nLocation: {redirectUri.AbsoluteUri}\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";

                await sslStream.WriteAsync(Encoding.ASCII.GetBytes(response), cancellationToken);
            });
        }

        public static X509Certificate2 CreateSelfSignedCertificate(string subjectName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subjectName);

            using var rsa = RSA.Create(2048);
            var request = new CertificateRequest(
                new X500DistinguishedName($"CN={subjectName}"),
                rsa,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509BasicConstraintsExtension(false, false, 0, false));
            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, false));
            request.CertificateExtensions.Add(
                new X509SubjectKeyIdentifierExtension(request.PublicKey, false));
            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    [new Oid("1.3.6.1.5.5.7.3.1")],
                    false));

            var sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddDnsName(subjectName);
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(
                DateTimeOffset.UtcNow.AddDays(-1),
                DateTimeOffset.UtcNow.AddDays(30));

            return X509CertificateLoader.LoadPkcs12(certificate.Export(X509ContentType.Pfx), null);
        }

        public async ValueTask DisposeAsync()
        {
            await _cancellationTokenSource.CancelAsync();
            _listener.Stop();

            try
            {
                await _acceptTask;
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
                _cancellationTokenSource.Dispose();
            }
        }

        private async Task AcceptClientAsync()
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                _cancellationTokenSource.Token,
                TestCancellationToken);

            using var client = await _listener.AcceptTcpClientAsync(linkedCts.Token);
            await _handler(client, linkedCts.Token);
        }

        private static async Task ReadHeadersAsync(Stream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[4096];
            using var ms = new MemoryStream();

            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
                if (bytesRead == 0)
                {
                    throw new InvalidOperationException("The client closed the TCP stream before sending HTTP headers");
                }

                await ms.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

                if (Encoding.ASCII.GetString(ms.ToArray()).Contains("\r\n\r\n", StringComparison.Ordinal))
                {
                    return;
                }
            }
        }
    }
}
