using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using RuriLib.Proxies;
using System.Text;
using RuriLib.Proxies.Exceptions;
using System.Collections.Generic;
using RuriLib.Http.Models;
using System.Linq;

namespace RuriLib.Http
{
    /// <summary>
    /// Custom implementation of an HttpClient.
    /// </summary>
    public class RLHttpClient : IDisposable
    {
        private readonly ProxyClient proxyClient;

        private Stream connectionCommonStream;
        private NetworkStream connectionNetworkStream;

        #region Properties
        /// <summary>
        /// The underlying proxy client.
        /// </summary>
        public ProxyClient ProxyClient => proxyClient;

        /// <summary>
        /// Gets the raw bytes of all the requests that were sent.
        /// </summary>
        public List<byte[]> RawRequests { get; } = new();

        /// <summary>
        /// Allow automatic redirection on 3xx reply.
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// The maximum number of times a request will be redirected.
        /// </summary>
        public int MaxNumberOfRedirects { get; set; }

        /// <summary>
        /// The allowed SSL or TLS protocols.
        /// </summary>
        public SslProtocols SslProtocols { get; set; } = SslProtocols.None;

        /// <summary>
        /// If true, <see cref="AllowedCipherSuites"/> will be used instead of the default ones.
        /// </summary>
        public bool UseCustomCipherSuites { get; set; } = false;

        /// <summary>
        /// The cipher suites to send to the server during the TLS handshake, in order.
        /// The default value of this property contains the cipher suites sent by Firefox as of 21 Dec 2020.
        /// </summary>
        public TlsCipherSuite[] AllowedCipherSuites { get; set; } = new TlsCipherSuite[]
        {
            TlsCipherSuite.TLS_AES_128_GCM_SHA256,
            TlsCipherSuite.TLS_CHACHA20_POLY1305_SHA256,
            TlsCipherSuite.TLS_AES_256_GCM_SHA384,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_GCM_SHA256,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_CHACHA20_POLY1305_SHA256,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_CHACHA20_POLY1305_SHA256,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_GCM_SHA384,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_256_CBC_SHA,
            TlsCipherSuite.TLS_ECDHE_ECDSA_WITH_AES_128_CBC_SHA,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_128_CBC_SHA,
            TlsCipherSuite.TLS_ECDHE_RSA_WITH_AES_256_CBC_SHA,
            TlsCipherSuite.TLS_RSA_WITH_AES_128_GCM_SHA256,
            TlsCipherSuite.TLS_RSA_WITH_AES_256_GCM_SHA384,
            TlsCipherSuite.TLS_RSA_WITH_AES_128_CBC_SHA,
            TlsCipherSuite.TLS_RSA_WITH_AES_256_CBC_SHA,
            TlsCipherSuite.TLS_RSA_WITH_3DES_EDE_CBC_SHA
        };

        /// <summary>
        /// Gets the type of decompression method used by the handler for automatic 
        /// decompression of the HTTP content response.
        /// </summary>
        /// <remarks>
        /// Support GZip and Deflate encoding automatically
        /// </remarks>
        public DecompressionMethods AutomaticDecompression => DecompressionMethods.GZip | DecompressionMethods.Deflate;

        /// <summary>
        /// Gets or sets delegate to verifies the remote Secure Sockets Layer (SSL) 
        /// certificate used for authentication.
        /// </summary>
        public RemoteCertificateValidationCallback ServerCertificateCustomValidationCallback { get; set; }

        /// <summary>
        /// Gets or sets the X509 certificate revocation mode.
        /// </summary>
        public X509RevocationMode CertRevocationMode { get; set; }
        #endregion

        /// <summary>
        /// Creates a new instance of <see cref="RLHttpClient"/> given a <paramref name="proxyClient"/>.
        /// </summary>
        public RLHttpClient(ProxyClient proxyClient)
        {
            this.proxyClient = proxyClient ?? throw new ArgumentNullException(nameof(proxyClient));
        }

        /// <summary>
        /// Asynchronously sends a <paramref name="request"/> and returns an <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
        public async Task<HttpResponse> SendAsync(HttpRequest request,
            CancellationToken cancellationToken = default, int redirects = 0)
        {
            if (redirects > MaxNumberOfRedirects)
            {
                throw new Exception("Maximum number of redirects exceeded");
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            await CreateConnection(request, cancellationToken).ConfigureAwait(false);
            await SendDataAsync(request, cancellationToken).ConfigureAwait(false);
            var responseMessage = await ReceiveDataAsync(request, cancellationToken).ConfigureAwait(false);

            // Optionally perform auto redirection on 3xx response
            if (((int)responseMessage.StatusCode) / 100 == 3 && AllowAutoRedirect)
            {
                // Compute the redirection URI
                var locationHeaderName = responseMessage.Headers.Keys
                    .First(k => k.Equals("Location", StringComparison.OrdinalIgnoreCase));

                Uri.TryCreate(responseMessage.Headers[locationHeaderName], UriKind.RelativeOrAbsolute, out var newLocation);

                var redirectUri = newLocation.IsAbsoluteUri
                    ? newLocation
                    : new Uri(request.Uri, newLocation);

                // If not 307, change the method to GET
                if (responseMessage.StatusCode != HttpStatusCode.RedirectKeepVerb)
                {
                    request.Method = HttpMethod.Get;
                    request.Content = null;
                }

                // Adjust the request if the host is different
                if (request.Uri.Host != redirectUri.Host)
                {
                    // This is needed otherwise if the Host header was set manually
                    // it will keep the previous one after a domain switch
                    if (request.HeaderExists("Host", out var hostHeaderName))
                    {
                        request.Headers.Remove(hostHeaderName);
                    }

                    // Remove additional headers that could cause trouble
                    request.Headers.Remove("Origin");
                }

                // Set the new URI
                request.Uri = redirectUri;

                // Perform a new request
                return await SendAsync(request, cancellationToken, redirects++).ConfigureAwait(false);
            }

            return responseMessage;
        }

        private async Task SendDataAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            var buffer = await request.GetBytesAsync(cancellationToken);
            await connectionCommonStream.WriteAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);

            RawRequests.Add(buffer);
        }

        private Task<HttpResponse> ReceiveDataAsync(HttpRequest request,
            CancellationToken cancellationToken) =>
            new HttpResponseBuilder().GetResponseAsync(request, connectionCommonStream, cancellationToken);

        private async Task CreateConnection(HttpRequest request, CancellationToken cancellationToken)
        {
            // Get the stream from the proxies TcpClient
            var uri = request.Uri;
            var tcpClient = await proxyClient.ConnectAsync(uri.Host, uri.Port, null, cancellationToken);
            connectionNetworkStream = tcpClient.GetStream();

            // If https, set up a TLS stream
            if (uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    SslStream sslStream;
                    sslStream = new SslStream(connectionNetworkStream, false, ServerCertificateCustomValidationCallback);

                    var sslOptions = new SslClientAuthenticationOptions
                    {
                        TargetHost = uri.Host,
                        EnabledSslProtocols = SslProtocols,
                        CertificateRevocationCheckMode = CertRevocationMode
                    };

                    if (UseCustomCipherSuites)
                        sslOptions.CipherSuitesPolicy = new CipherSuitesPolicy(AllowedCipherSuites);

                    await sslStream.AuthenticateAsClientAsync(sslOptions, cancellationToken);
                    connectionCommonStream = sslStream;
                }
                catch (Exception ex)
                {
                    if (ex is IOException || ex is AuthenticationException)
                    {
                        throw new ProxyException("Failed SSL connect");
                    }

                    throw;
                }
            }
            else
            {
                connectionCommonStream = connectionNetworkStream;
            }
        }

        public void Dispose()
        {
            connectionCommonStream?.Dispose();
            connectionNetworkStream?.Dispose();
        }
    }
}