using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System;
using RuriLib.Proxies.Exceptions;
using RuriLib.Proxies.Clients;
using System.Security;

namespace RuriLib.Proxies
{
    public abstract class ProxyClient
    {
        /// <summary>
        /// The proxy settings.
        /// </summary>
        public ProxySettings Settings { get; }

        public ProxyClient(ProxySettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// /// <summary>
        /// Create a proxied <see cref="TcpClient"/> to the destination host.
        /// </summary>
        /// <param name="destinationHost">The host you want to connect to</param>
        /// <param name="destinationPort">The port on which the host is listening</param>
        /// <param name="cancellationToken">A token to cancel the connection attempt</param>
        /// <param name="tcpClient">A <see cref="TcpClient"/> instance (if null, a new one will be created)</param>
        /// <exception cref="ArgumentException">Value of <paramref name="destinationHost"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Value of <paramref name="destinationPort"/> less than 1 or greater than 65535.</exception>
        /// <exception cref="ProxyException">Error while working with the proxy.</exception>
        public async Task<TcpClient> ConnectAsync(string destinationHost, int destinationPort, TcpClient tcpClient = null,
            CancellationToken cancellationToken = default)
        {
            var client = tcpClient ?? new TcpClient()
            {
                ReceiveTimeout = (int)Settings.ReadWriteTimeOut.TotalMilliseconds,
                SendTimeout = (int)Settings.ReadWriteTimeOut.TotalMilliseconds
            };

            Exception connectException = null;
            var connectDoneEvent = new ManualResetEventSlim();

            var host = Settings.Host;
            var port = Settings.Port;

            // NoProxy case, connect directly to the server without proxy
            if (this is NoProxyClient)
            {
                host = destinationHost;
                port = destinationPort;
            }

            // Try to connect to the proxy (or directly to the server in the NoProxy case)
            try
            {
                client.BeginConnect(host, port, new AsyncCallback(
                    (ar) =>
                    {
                        if (client.Client != null)
                        {
                            try
                            {
                                client.EndConnect(ar);
                            }
                            catch (Exception ex)
                            {
                                connectException = ex;
                            }

                            connectDoneEvent.Set();
                        }
                    }), client
                );
            }
            catch (Exception ex)
            {
                client.Close();

                if (ex is SocketException || ex is SecurityException)
                {
                    throw new ProxyException($"Failed to connect to {(this is NoProxyClient ? "server" : "proxy-server")}", ex);
                }

                throw;
            }

            // Wait until the connection is completed. If it cannot be completed, throw.
            if (!connectDoneEvent.Wait(Settings.ConnectTimeout, cancellationToken))
            {
                client.Close();
                throw new ProxyException($"Failed to connect to {(this is NoProxyClient ? "server" : "proxy-server")}");
            }

            // If the connection was completed with an exception, throw.
            if (connectException != null)
            {
                client.Close();

                if (connectException is SocketException)
                {
                    throw new ProxyException($"Failed to connect to {(this is NoProxyClient ? "server" : "proxy-server")}", connectException);
                }
                else
                {
                    throw connectException;
                }
            }

            // If the client isn't connected, throw.
            if (!client.Connected)
            {
                client.Close();
                throw new ProxyException($"Failed to connect to {(this is NoProxyClient ? "server" : "proxy-server")}");
            }

            client.SendTimeout = (int)Settings.ReadWriteTimeOut.TotalMilliseconds;
            client.ReceiveTimeout = (int)Settings.ReadWriteTimeOut.TotalMilliseconds;

            await CreateConnectionAsync(client,
                destinationHost,
                destinationPort,
                cancellationToken);

            return client;
        }

        protected virtual Task CreateConnectionAsync(TcpClient tcpClient, string destinationHost, int destinationPort,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
