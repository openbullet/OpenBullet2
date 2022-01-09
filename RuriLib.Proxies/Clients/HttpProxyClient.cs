using System;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RuriLib.Proxies.Helpers;
using RuriLib.Proxies.Exceptions;

namespace RuriLib.Proxies.Clients
{
    /// <summary>
    /// A client that provides proxies connections via HTTP proxies.
    /// </summary>
    public class HttpProxyClient : ProxyClient
    {
        /// <summary>
        /// The HTTP version to send in the first line of the request to the proxy.
        /// By default it's 1.1
        /// </summary>
        public string ProtocolVersion { get; set; } = "1.1";

        /// <summary>
        /// Creates an HTTP proxy client given the proxy <paramref name="settings"/>.
        /// </summary>
        public HttpProxyClient(ProxySettings settings) : base(settings)
        {

        }

        /// <inheritdoc/>
        protected async override Task CreateConnectionAsync(TcpClient client, string destinationHost, int destinationPort,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(destinationHost))
            {
                throw new ArgumentException(null, nameof(destinationHost));
            }

            if (!PortHelper.ValidateTcpPort(destinationPort))
            {
                throw new ArgumentOutOfRangeException(nameof(destinationPort));
            }

            if (client == null || !client.Connected)
            {
                throw new SocketException();
            }

            HttpStatusCode statusCode;

            try
            {
                var nStream = client.GetStream();

                await RequestConnectionAsync(nStream, destinationHost, destinationPort, cancellationToken).ConfigureAwait(false);
                statusCode = await ReceiveResponseAsync(nStream, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                client.Close();

                if (ex is IOException || ex is SocketException)
                {
                    throw new ProxyException("Error while working with proxy", ex);
                }

                throw;
            }

            if (statusCode != HttpStatusCode.OK)
            {
                client.Close();
                throw new ProxyException("The proxy didn't reply with 200 OK");
            }
        }

        private async Task RequestConnectionAsync(Stream nStream, string destinationHost, int destinationPort,
            CancellationToken cancellationToken = default)
        {
            var commandBuilder = new StringBuilder();

            commandBuilder.AppendFormat("CONNECT {0}:{1} HTTP/{2}\r\n{3}\r\n", destinationHost, destinationPort, ProtocolVersion, GenerateAuthorizationHeader());

            var buffer = Encoding.ASCII.GetBytes(commandBuilder.ToString());

            await nStream.WriteAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
        }

        private string GenerateAuthorizationHeader()
        {
            if (Settings.Credentials == null || string.IsNullOrEmpty(Settings.Credentials.UserName))
            {
                return string.Empty;
            }

            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                $"{Settings.Credentials.UserName}:{Settings.Credentials.Password}"));

            return $"Proxy-Authorization: Basic {data}\r\n";
        }

        private static async Task<HttpStatusCode> ReceiveResponseAsync(NetworkStream nStream, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[50];
            var responseBuilder = new StringBuilder();

            using var waitCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(nStream.ReadTimeout));

            while (!nStream.DataAvailable)
            {
                // Throw default exception if the operation was cancelled by the user
                cancellationToken.ThrowIfCancellationRequested();

                // Throw a custom exception if we timed out
                if (waitCts.Token.IsCancellationRequested)
                    throw new ProxyException("Timed out while waiting for data from proxy");

                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }

            do
            {
                var bytesRead = await nStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
                responseBuilder.Append(Encoding.ASCII.GetString(buffer, 0, bytesRead));
            }
            while (nStream.DataAvailable);

            var response = responseBuilder.ToString();

            if (response.Length == 0)
                throw new ProxyException("Received empty response");

            // Check if the response is a correct HTTP response
            var match = Regex.Match(response, "HTTP/[0-9\\.]* ([0-9]{3})");

            if (!match.Success)
                throw new ProxyException("Received wrong HTTP response from proxy");

            if (!Enum.TryParse(match.Groups[1].Value, out HttpStatusCode statusCode))
                throw new ProxyException("Invalid HTTP status code");

            return statusCode;
        }
    }
}
