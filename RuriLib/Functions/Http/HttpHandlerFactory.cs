using RuriLib.Models.Proxies;
using SocksSharp;
using SocksSharp.Proxy;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;

namespace RuriLib.Functions.Http
{
    public class HttpHandlerFactory
    {
        public CookieContainer Cookies { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(20);
        public bool AutoRedirect { get; set; } = true;
        public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SystemDefault;
        public bool UseCustomCipherSuites { get; set; } = false;
        public TlsCipherSuite[] CustomCipherSuites { get; set; } = new TlsCipherSuite[]
        {
            // Default Firefox suites
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

        public HttpMessageHandler GetHandler(Proxy proxy)
        {
            HttpMessageHandler handler = null;

            if (proxy == null)
            {
                handler = new ProxyClientHandler<NoProxy>(new ProxySettings())
                {
                    AllowAutoRedirect = AutoRedirect,
                    CookieContainer = Cookies,
                    SslProtocols = ToSslProtocols(SecurityProtocol),
                    UseCustomCipherSuites = UseCustomCipherSuites,
                    AllowedCipherSuites = CustomCipherSuites
                };
            }
            else
            {
                var settings = new ProxySettings()
                {
                    Host = proxy.Host,
                    Port = proxy.Port,
                    ConnectTimeout = (int)Timeout.TotalMilliseconds,
                    ReadWriteTimeOut = (int)Timeout.TotalMilliseconds
                };

                if (proxy.NeedsAuthentication)
                    settings.SetCredential(proxy.Username, proxy.Password);

                handler = proxy.Type switch
                {
                    ProxyType.Http => new ProxyClientHandler<SocksSharp.Proxy.Http>(settings)
                    {
                        AllowAutoRedirect = AutoRedirect,
                        CookieContainer = Cookies,
                        SslProtocols = ToSslProtocols(SecurityProtocol),
                        UseCustomCipherSuites = UseCustomCipherSuites,
                        AllowedCipherSuites = CustomCipherSuites
                    },

                    ProxyType.Socks4 => new ProxyClientHandler<Socks4>(settings)
                    {
                        AllowAutoRedirect = AutoRedirect,
                        CookieContainer = Cookies,
                        SslProtocols = ToSslProtocols(SecurityProtocol),
                        UseCustomCipherSuites = UseCustomCipherSuites,
                        AllowedCipherSuites = CustomCipherSuites
                    },

                    ProxyType.Socks5 => new ProxyClientHandler<Socks5>(settings)
                    {
                        AllowAutoRedirect = AutoRedirect,
                        CookieContainer = Cookies,
                        SslProtocols = ToSslProtocols(SecurityProtocol),
                        UseCustomCipherSuites = UseCustomCipherSuites,
                        AllowedCipherSuites = CustomCipherSuites
                    },

                    _ => throw new NotImplementedException()
                };
            }

            return handler;
        }

        /// <summary>
        /// Converts the <paramref name="protocol"/> to an SslProtocols enum. Multiple protocols are not supported and SystemDefault is None.
        /// </summary>
        private SslProtocols ToSslProtocols(SecurityProtocol protocol)
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
