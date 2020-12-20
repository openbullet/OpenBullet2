using RuriLib.Models.Proxies;
using SocksSharp.Proxy;
using System;
using System.Net;
using System.Net.Sockets;

namespace RuriLib.Functions.Tcp
{
    public static class TcpFactory
    {
        /// <summary>
        /// Creates a socket that talks to the given <paramref name="host"/> on the given <paramref name="port"/>
        /// (optionally through a proxy) and returns the <see cref="NetworkStream"/>.
        /// </summary>
        public static NetworkStream GetNetworkStream(string host, int port, TimeSpan timeout, Proxy proxy = null)
        {
            if (proxy == null)
            {
                var client = new TcpClient(host, port);
                return client.GetStream();
            }

            switch (proxy.Type)
            {
                case ProxyType.Http:
                    var http = new ProxyClient<SocksSharp.Proxy.Http>();
                    http.Settings = MakeSettings(proxy, timeout);
                    return http.GetDestinationStream(host, port);

                case ProxyType.Socks4:
                    var socks4 = new ProxyClient<Socks4>();
                    socks4.Settings = MakeSettings(proxy, timeout);
                    return socks4.GetDestinationStream(host, port);

                case ProxyType.Socks5:
                    var socks5 = new ProxyClient<Socks5>();
                    socks5.Settings = MakeSettings(proxy, timeout);
                    return socks5.GetDestinationStream(host, port);

                default:
                    throw new NotImplementedException();
            }
        }

        private static ProxySettings MakeSettings(Proxy proxy, TimeSpan timeout)
        {
            return new ProxySettings
            {
                Host = proxy.Host,
                Port = proxy.Port,
                ConnectTimeout = (int)timeout.TotalMilliseconds,
                ReadWriteTimeOut = (int)timeout.TotalMilliseconds,
                Credentials = new NetworkCredential(proxy.Username, proxy.Password)
            };
        }
    }
}
