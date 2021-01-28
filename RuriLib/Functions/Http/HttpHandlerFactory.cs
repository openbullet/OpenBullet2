using RuriLib.Models.Proxies;
using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Net.Http;
using System.Security.Authentication;

namespace RuriLib.Functions.Http
{
    public class HttpHandlerFactory
    {
        public static HttpMessageHandler GetHandler(Proxy proxy, HttpHandlerOptions options)
        {
            HttpMessageHandler handler;

            if (proxy == null)
            {
                handler = new ProxyClientHandler<NoProxy>(new ProxySettings())
                {
                    AllowAutoRedirect = options.AutoRedirect,
                    CookieContainer = options.Cookies,
                    UseCookies = options.Cookies != null,
                    SslProtocols = ToSslProtocols(options.SecurityProtocol),
                    UseCustomCipherSuites = options.UseCustomCipherSuites,
                    AllowedCipherSuites = options.CustomCipherSuites,
                    CertRevocationMode = options.CertRevocationMode
                };
            }
            else
            {
                var settings = new ProxySettings()
                {
                    Host = proxy.Host,
                    Port = proxy.Port,
                    ConnectTimeout = (int)options.ConnectTimeout.TotalMilliseconds,
                    ReadWriteTimeOut = (int)options.ReadWriteTimeout.TotalMilliseconds
                };

                if (proxy.NeedsAuthentication)
                    settings.SetCredential(proxy.Username, proxy.Password);

                handler = proxy.Type switch
                {
                    ProxyType.Http => new ProxyClientHandler<SocksSharp.Proxy.Http>(settings)
                    {
                        AllowAutoRedirect = options.AutoRedirect,
                        CookieContainer = options.Cookies,
                        UseCookies = options.Cookies != null,
                        SslProtocols = ToSslProtocols(options.SecurityProtocol),
                        UseCustomCipherSuites = options.UseCustomCipherSuites,
                        AllowedCipherSuites = options.CustomCipherSuites,
                        CertRevocationMode = options.CertRevocationMode
                    },

                    ProxyType.Socks4 => new ProxyClientHandler<Socks4>(settings)
                    {
                        AllowAutoRedirect = options.AutoRedirect,
                        CookieContainer = options.Cookies,
                        UseCookies = options.Cookies != null,
                        SslProtocols = ToSslProtocols(options.SecurityProtocol),
                        UseCustomCipherSuites = options.UseCustomCipherSuites,
                        AllowedCipherSuites = options.CustomCipherSuites,
                        CertRevocationMode = options.CertRevocationMode
                    },

                    ProxyType.Socks5 => new ProxyClientHandler<Socks5>(settings)
                    {
                        AllowAutoRedirect = options.AutoRedirect,
                        CookieContainer = options.Cookies,
                        UseCookies = options.Cookies != null,
                        SslProtocols = ToSslProtocols(options.SecurityProtocol),
                        UseCustomCipherSuites = options.UseCustomCipherSuites,
                        AllowedCipherSuites = options.CustomCipherSuites,
                        CertRevocationMode = options.CertRevocationMode
                    },

                    _ => throw new NotImplementedException()
                };
            }

            return handler;
        }

        /// <summary>
        /// Converts the <paramref name="protocol"/> to an SslProtocols enum. Multiple protocols are not supported and SystemDefault is None.
        /// </summary>
        private static SslProtocols ToSslProtocols(SecurityProtocol protocol)
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
