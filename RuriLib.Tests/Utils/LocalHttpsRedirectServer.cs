using System;
using System.IO;
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

internal sealed class LocalHttpsRedirectServer : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private readonly X509Certificate2 certificate;
    private readonly Uri redirectUri;
    private readonly Task acceptTask;

    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public LocalHttpsRedirectServer(X509Certificate2 certificate, Uri redirectUri)
    {
        this.certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
        this.redirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));

        listener.Start();
        acceptTask = Task.Run(AcceptClientAsync, CancellationToken.None);
    }

    public Uri Uri => new($"https://localhost:{Port}/");

    private int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

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
        catch (IOException)
        {
        }
        catch (AuthenticationException)
        {
        }
        finally
        {
            certificate.Dispose();
            cancellationTokenSource.Dispose();
        }
    }

    private async Task AcceptClientAsync()
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationTokenSource.Token,
            TestCancellationToken);

        using var client = await listener.AcceptTcpClientAsync(linkedCts.Token);
        await using var networkStream = client.GetStream();
        await using var sslStream = new SslStream(networkStream, leaveInnerStreamOpen: false);

        await sslStream.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
        {
            ServerCertificate = certificate,
            ClientCertificateRequired = false,
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        }, linkedCts.Token);

        await ReadHeadersAsync(sslStream, linkedCts.Token);

        var response =
            $"HTTP/1.1 302 Found\r\nLocation: {redirectUri.AbsoluteUri}\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";

        await sslStream.WriteAsync(Encoding.ASCII.GetBytes(response), linkedCts.Token);
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
