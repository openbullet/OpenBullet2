using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Utils;

internal sealed class TestTcpServer : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly Func<TcpClient, CancellationToken, Task> handler;
    private readonly Task acceptTask;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    private TestTcpServer(Func<TcpClient, CancellationToken, Task> handler)
    {
        this.handler = handler ?? throw new ArgumentNullException(nameof(handler));

        listener.Start();
        acceptTask = Task.Run(AcceptClientAsync, CancellationToken.None);
    }

    public string Host => "127.0.0.1";

    public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    public static TestTcpServer CreateEchoServer()
        => new(async (stream, cancellationToken) =>
        {
            await using var networkStream = stream.GetStream();
            var bytes = await ReadOnceAsync(networkStream, cancellationToken);
            await networkStream.WriteAsync(bytes, cancellationToken);
        });

    public static TestTcpServer CreateResponseServer(string response, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(encoding);

        return new(async (stream, cancellationToken) =>
        {
            await using var networkStream = stream.GetStream();
            var bytes = encoding.GetBytes(response);
            await networkStream.WriteAsync(bytes, cancellationToken);
        });
    }

    public static TestTcpServer CreateResponseServer(byte[] response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new(async (stream, cancellationToken) =>
        {
            await using var networkStream = stream.GetStream();
            await networkStream.WriteAsync(response, cancellationToken);
        });
    }

    public static TestTcpServer CreateTlsEchoServer(X509Certificate2 certificate)
    {
        ArgumentNullException.ThrowIfNull(certificate);

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

            var bytes = await ReadOnceAsync(sslStream, cancellationToken);
            await sslStream.WriteAsync(bytes, cancellationToken);
        });
    }

    public static TestTcpServer CreateHttpServer(string responseBody, bool gzip = false, bool keepAlive = false,
        bool chunked = false, TimeSpan? keepAliveDuration = null)
    {
        ArgumentNullException.ThrowIfNull(responseBody);

        return new(async (client, cancellationToken) =>
        {
            await using var networkStream = client.GetStream();
            await ReadHeadersAsync(networkStream, cancellationToken);

            var payload = Encoding.UTF8.GetBytes(responseBody);
            if (gzip)
            {
                payload = Compress(payload);
            }

            var headers = "HTTP/1.1 200 OK\r\n";

            if (chunked)
            {
                headers += "Transfer-Encoding: chunked\r\n";
            }
            else
            {
                headers += $"Content-Length: {payload.Length}\r\n";
            }

            headers += keepAlive ? "Connection: keep-alive\r\n" : "Connection: close\r\n";

            if (gzip)
            {
                headers += "Content-Encoding: gzip\r\n";
            }

            headers += "\r\n";

            await networkStream.WriteAsync(Encoding.UTF8.GetBytes(headers), cancellationToken);

            if (chunked)
            {
                var chunkHeader = Encoding.ASCII.GetBytes($"{payload.Length:X}\r\n");
                await networkStream.WriteAsync(chunkHeader, cancellationToken);
                await networkStream.WriteAsync(payload, cancellationToken);
                await networkStream.WriteAsync("\r\n0\r\n\r\n"u8.ToArray(), cancellationToken);
            }
            else
            {
                await networkStream.WriteAsync(payload, cancellationToken);
            }

            if (keepAlive)
            {
                await Task.Delay(keepAliveDuration ?? TimeSpan.FromSeconds(5), cancellationToken);
            }
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
        cancellationTokenSource.Cancel();
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
            cancellationTokenSource.Dispose();
        }
    }

    private async Task AcceptClientAsync()
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationTokenSource.Token,
            TestCancellationToken);

        using var client = await listener.AcceptTcpClientAsync(linkedCts.Token);
        await handler(client, linkedCts.Token);
    }

    private static async Task<byte[]> ReadOnceAsync(Stream stream, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        return new ArraySegment<byte>(buffer, 0, bytesRead).ToArray();
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
                throw new InvalidOperationException("The client closed the TCP stream before sending HTTP headers");
            }

            await ms.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

            if (Encoding.ASCII.GetString(ms.ToArray()).Contains("\r\n\r\n", StringComparison.Ordinal))
            {
                return;
            }
        }
    }

    private static byte[] Compress(byte[] payload)
    {
        using var ms = new MemoryStream();
        using (var gzip = new GZipStream(ms, CompressionMode.Compress, leaveOpen: true))
        {
            gzip.Write(payload, 0, payload.Length);
        }

        return ms.ToArray();
    }
}
