using RuriLib.Proxies.Helpers;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Proxies.Clients
{
    /// <summary>
    /// A dummy client that does not proxy the connection.
    /// </summary>
    public class NoProxyClient : ProxyClient
    {
        /// <summary>
        /// Provides unproxied connections.
        /// </summary>
        public NoProxyClient(ProxySettings settings = null) : base(settings ?? new ProxySettings())
        {

        }

        /// <inheritdoc/>
        protected override Task CreateConnectionAsync(TcpClient client, string destinationHost, int destinationPort,
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

            return Task.CompletedTask;
        }
    }
}
