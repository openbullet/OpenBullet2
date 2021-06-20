using RuriLib.Http;
using RuriLib.Models.Proxies;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Authentication;

namespace RuriLib.Functions.Http
{
    public class HttpFactory
    {
        public static ProxyClientHandler GetProxiedHandler(Proxy proxy, HttpOptions options, CookieContainer cookies)
        {
            var client = GetProxyClient(proxy, options);

            return new ProxyClientHandler(client)
            {
                AllowAutoRedirect = options.AutoRedirect,
                MaxNumberOfRedirects = options.MaxNumberOfRedirects,
                CookieContainer = cookies,
                UseCookies = cookies != null,
                SslProtocols = ToSslProtocols(options.SecurityProtocol),
                UseCustomCipherSuites = options.UseCustomCipherSuites,
                AllowedCipherSuites = options.CustomCipherSuites,
                CertRevocationMode = options.CertRevocationMode,
                ReadResponseContent = options.ReadResponseContent
            };
        }

        public static RLHttpClient GetRLHttpClient(Proxy proxy, HttpOptions options)
        {
            var client = GetProxyClient(proxy, options);

            return new RLHttpClient(client)
            {
                AllowAutoRedirect = options.AutoRedirect,
                MaxNumberOfRedirects = options.MaxNumberOfRedirects,
                SslProtocols = ToSslProtocols(options.SecurityProtocol),
                UseCustomCipherSuites = options.UseCustomCipherSuites,
                AllowedCipherSuites = options.CustomCipherSuites,
                CertRevocationMode = options.CertRevocationMode,
                ReadResponseContent = options.ReadResponseContent
            };
        }

        private static ProxyClient GetProxyClient(Proxy proxy, HttpOptions options)
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
                    ConnectTimeout = options.ConnectTimeout,
                    ReadWriteTimeOut = options.ReadWriteTimeout
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

            return client;
        }

        /// <summary>
        /// Converts the <paramref name="protocol"/> to an SslProtocols enum. Multiple protocols are not supported and SystemDefault is None.
        /// </summary>
        private static SslProtocols ToSslProtocols(SecurityProtocol protocol)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && protocol == SecurityProtocol.TLS13)
            {
                throw new Exception("To use TLS 1.3 on Windows please use the SystemDefault option");
            }

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
