using RuriLib.Models.Proxies;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Tcp
{
    public static class TcpClientFactory
    {
        /// <summary>
        /// Creates a socket that talks to the given <paramref name="host"/> on the given <paramref name="port"/>
        /// (optionally through a proxy) and returns the <see cref="NetworkStream"/>.
        /// </summary>
        public static Task<TcpClient> GetClientAsync(string host, int port, TimeSpan timeout, Proxy proxy = null,
            CancellationToken cancellationToken = default)
        {
            ProxyClient client;

            if (proxy == null)
            {
                client = new NoProxyClient(new ProxySettings());
            }
            else
            {
                var settings = new ProxySettings()
                {
                    Host = proxy.Host,
                    Port = proxy.Port,
                    ConnectTimeout = timeout,
                    ReadWriteTimeOut = timeout
                };

                if (proxy.NeedsAuthentication)
                {
                    settings.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
                }

                client = proxy.Type switch
                {
                    ProxyType.Http => new HttpProxyClient(settings),
                    ProxyType.Socks4 => new Socks4ProxyClient(settings),
                    ProxyType.Socks4a => new Socks4aProxyClient(settings),
                    ProxyType.Socks5 => new Socks5ProxyClient(settings),
                    _ => throw new NotImplementedException()
                };
            }

            return client.ConnectAsync(host, port, null, cancellationToken);
        }
    }
}
