using RuriLib.Proxies.Exceptions;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Proxies.Clients;

/// <summary>
/// A client that provides proxy connections via HTTPS proxies.
/// </summary>
public class HttpsProxyClient(ProxySettings settings) : HttpProxyClient(settings)
{
    /// <inheritdoc />
    protected override async Task<Stream> CreateProxyTransportStreamAsync(TcpClient tcpClient, Stream stream,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var sslStream = new SslStream(stream, false, Settings.ProxyCertificateValidationCallback);
            var sslOptions = new SslClientAuthenticationOptions
            {
                TargetHost = Settings.Host,
                CertificateRevocationCheckMode = Settings.ProxyCertRevocationMode,
                ApplicationProtocols = [SslApplicationProtocol.Http11]
            };

            await sslStream.AuthenticateAsClientAsync(sslOptions, cancellationToken).ConfigureAwait(false);
            return sslStream;
        }
        catch (Exception ex)
        {
            tcpClient.Close();

            if (ex is IOException or AuthenticationException)
            {
                throw new BadProxyException("Failed TLS handshake with HTTPS proxy", ex);
            }

            throw;
        }
    }
}
