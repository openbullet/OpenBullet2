using RuriLib.Models.Proxies;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Tcp;

/// <summary>
/// Creates TCP clients with optional proxy support.
/// </summary>
public static class TcpClientFactory
{
    /// <summary>
    /// Creates a TCP client that talks to the given <paramref name="host"/> on the given <paramref name="port"/>
    /// optionally through a proxy.
    /// </summary>
    /// <param name="host">The destination host.</param>
    /// <param name="port">The destination port.</param>
    /// <param name="timeout">The connect/read-write timeout.</param>
    /// <param name="proxy">The optional proxy to use.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connected <see cref="TcpClient"/>.</returns>
    public static Task<TcpClient> GetClientAsync(string host, int port, TimeSpan timeout, Proxy? proxy = null,
        CancellationToken cancellationToken = default)
    {
        ProxyClient client;

        if (proxy is null)
        {
            client = new NoProxyClient(new ProxySettings
            {
                ConnectTimeout = timeout,
                ReadWriteTimeOut = timeout
            });
        }
        else
        {
            var settings = new ProxySettings
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
