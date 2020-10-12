using RuriLib.Models.Proxies;
using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;

namespace RuriLib.Functions.Http
{
    public static class HttpHandlerFactory
    {
        public static HttpMessageHandler GetHandler(Proxy proxy, CookieContainer cookies, TimeSpan timeout,
            bool autoRedirect = true, SecurityProtocol securityProtocol = SecurityProtocol.SystemDefault)
        {
            if (proxy == null)
            {
                return new HttpClientHandler
                {
                    AllowAutoRedirect = autoRedirect,
                    CookieContainer = cookies,
                    UseProxy = false,
                    SslProtocols = securityProtocol.ToSslProtocols()
                };
            }

            var settings = new ProxySettings()
            {
                Host = proxy.Host,
                Port = proxy.Port,
                ConnectTimeout = (int)timeout.TotalMilliseconds,
                ReadWriteTimeOut = (int)timeout.TotalMilliseconds
            };

            if (proxy.NeedsAuthentication)
                settings.SetCredential(proxy.Username, proxy.Password);

            return proxy.Type switch
            {
                ProxyType.Http => MakeHttpHandler(settings, cookies, autoRedirect, securityProtocol),
                ProxyType.Socks4 => MakeSocks4Handler(settings, cookies, autoRedirect, securityProtocol),
                ProxyType.Socks5 => MakeSocks5Handler(settings, cookies, autoRedirect, securityProtocol),
                _ => throw new NotImplementedException()
            };
        }

        private static ProxyClientHandler<SocksSharp.Proxy.Http> MakeHttpHandler(ProxySettings settings,
            CookieContainer cookies, bool autoRedirect, SecurityProtocol securityProtocol)
        {
            return new ProxyClientHandler<SocksSharp.Proxy.Http>(settings)
            {
                AllowAutoRedirect = autoRedirect,
                CookieContainer = cookies,
                SslProtocols = securityProtocol.ToSslProtocols()
            };
        }

        private static ProxyClientHandler<Socks4> MakeSocks4Handler(ProxySettings settings,
            CookieContainer cookies, bool autoRedirect, SecurityProtocol securityProtocol)
        {
            return new ProxyClientHandler<Socks4>(settings)
            {
                AllowAutoRedirect = autoRedirect,
                CookieContainer = cookies,
                SslProtocols = securityProtocol.ToSslProtocols()
            };
        }

        private static ProxyClientHandler<Socks5> MakeSocks5Handler(ProxySettings settings,
            CookieContainer cookies, bool autoRedirect, SecurityProtocol securityProtocol)
        {
            return new ProxyClientHandler<Socks5>(settings)
            {
                AllowAutoRedirect = autoRedirect,
                CookieContainer = cookies,
                SslProtocols = securityProtocol.ToSslProtocols()
            };
        }

        /// <summary>
        /// Converts the <paramref name="protocol"/> to an SslProtocols enum. Multiple protocols are not supported and SystemDefault is None.
        /// </summary>
        private static SslProtocols ToSslProtocols(this SecurityProtocol protocol)
        {
            return protocol switch
            {
                SecurityProtocol.SystemDefault => SslProtocols.None,
                SecurityProtocol.TLS10 => SslProtocols.Tls,
                SecurityProtocol.TLS11 => SslProtocols.Tls11,
                SecurityProtocol.TLS12 => SslProtocols.Tls12,
                SecurityProtocol.TLS13 => SslProtocols.Tls13,
                _ => throw new Exception("Protocol not supported"),
            };
        }
    }
}
