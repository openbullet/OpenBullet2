using RuriLib.Http;
using RuriLib.Models.Proxies;
using RuriLib.Proxies;
using RuriLib.Proxies.Clients;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace RuriLib.Functions.Http;

/// <summary>
/// Provides methods to create HTTP clients with different configurations.
/// </summary>
public class HttpFactory
{
    /// <summary>
    /// Creates an <see cref="ProxyClientHandler"/> with the given
    /// <paramref name="proxy"/>, <paramref name="options"/> and
    /// <paramref name="cookies"/>.
    /// </summary>
    public static ProxyClientHandler GetProxiedHandler(
        Proxy? proxy, HttpOptions options, CookieContainer? cookies)
    {
        var client = GetProxyClient(proxy, options);

        return new ProxyClientHandler(client)
        {
            AllowAutoRedirect = options.AutoRedirect,
            MaxNumberOfRedirects = options.MaxNumberOfRedirects,
            CookieContainer = cookies ?? new CookieContainer(),
            UseCookies = cookies != null,
            SslProtocols = ToSslProtocols(options.SecurityProtocol),
            UseCustomCipherSuites = options.UseCustomCipherSuites,
            AllowedCipherSuites = options.CustomCipherSuites,
            CertRevocationMode = options.CertRevocationMode,
            ReadResponseContent = options.ReadResponseContent
        };
    }

    /// <summary>
    /// Creates an <see cref="RLHttpClient"/> with the given
    /// <paramref name="proxy"/> and <paramref name="options"/>.
    /// </summary>
    public static RLHttpClient GetRLHttpClient(Proxy? proxy, HttpOptions options)
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

    /// <summary>
    /// Creates an <see cref="HttpClient"/> with the given
    /// <paramref name="proxy"/>, <paramref name="options"/> and
    /// <paramref name="cookieContainer"/>.
    /// </summary>
    public static HttpClient GetHttpClient(Proxy? proxy, HttpOptions options, CookieContainer cookieContainer)
    {
        var handler = GetHttpMessageHandler(proxy, options, cookieContainer);
            
        return new HttpClient(handler)
        {
            Timeout = options.ReadWriteTimeout
        };
    }

    private static ProxyClient GetProxyClient(Proxy? proxy, HttpOptions options)
    {
        ProxyClient client;

        if (proxy is null)
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

    private static HttpMessageHandler GetHttpMessageHandler(Proxy? proxy, HttpOptions options, CookieContainer cookieContainer)
    {
        HttpMessageHandler handler;

        if (proxy is null)
        {
            handler = new HttpClientHandler();
        }
        else
        {
            handler = proxy.Type switch
            {
                ProxyType.Http => new HttpClientHandler
                {
                    Proxy = GetWebProxy(proxy)
                },
                ProxyType.Socks4 or ProxyType.Socks4a or ProxyType.Socks5 => new SocketsHttpHandler
                {
                    Proxy = GetWebProxy(proxy)
                },
                _ => throw new NotImplementedException()
            };
        }

        return ConfigureHttpMessageHandler(handler, options, cookieContainer);
    }

    private static WebProxy GetWebProxy(Proxy proxy)
    {
        var proxyCredentials = proxy.NeedsAuthentication
            ? new NetworkCredential(proxy.Username, proxy.Password)
            : null;

        var address = proxy.Type switch
        {
            ProxyType.Http => $"http://{proxy.Host}:{proxy.Port}",
            ProxyType.Socks4 => $"socks4://{proxy.Host}:{proxy.Port}",
            ProxyType.Socks4a => $"socks4a://{proxy.Host}:{proxy.Port}",
            ProxyType.Socks5 => $"socks5://{proxy.Host}:{proxy.Port}",
            _ => throw new NotImplementedException(),
        };

        return new WebProxy(address, true, null, proxyCredentials);
    }

    private static HttpMessageHandler ConfigureHttpMessageHandler(HttpMessageHandler handler, HttpOptions options, CookieContainer cookieContainer)
    {
        if (options.UseCustomCipherSuites && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new NotSupportedException("Custom cipher suites are not supported on Windows");
        }
            
        var sslOptions = new SslClientAuthenticationOptions
        {
            CertificateRevocationCheckMode = options.CertRevocationMode,
            EnabledSslProtocols = ToSslProtocols(options.SecurityProtocol),
            CipherSuitesPolicy = options.UseCustomCipherSuites
                ? new CipherSuitesPolicy(options.CustomCipherSuites)
                : null
        };

        if (handler is HttpClientHandler httpHandler)
        {
            httpHandler.MaxAutomaticRedirections = options.MaxNumberOfRedirects;
            httpHandler.AllowAutoRedirect = options.AutoRedirect;
            httpHandler.SslProtocols = ToSslProtocols(options.SecurityProtocol);
            httpHandler.CheckCertificateRevocationList = options.CertRevocationMode == X509RevocationMode.Online;
            httpHandler.UseCookies = true;
            httpHandler.CookieContainer = cookieContainer;

            // Hack to modify the SSL options
            var underlyingHandler = (dynamic)httpHandler.GetType().InvokeMember("_underlyingHandler",
                BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance, null, httpHandler, null)!;

            underlyingHandler.SslOptions = sslOptions;
        }
        else if (handler is SocketsHttpHandler socksHandler)
        {
            socksHandler.MaxAutomaticRedirections = options.MaxNumberOfRedirects;
            socksHandler.AllowAutoRedirect = options.AutoRedirect;
            socksHandler.SslOptions = sslOptions;
            socksHandler.ConnectTimeout = options.ConnectTimeout;
            socksHandler.ResponseDrainTimeout = options.ReadWriteTimeout;
            socksHandler.UseCookies = true;
            socksHandler.CookieContainer = cookieContainer;
        }

        return handler;
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
#pragma warning disable SYSLIB0039
            SecurityProtocol.TLS10 => SslProtocols.Tls,
            SecurityProtocol.TLS11 => SslProtocols.Tls11,
#pragma warning restore SYSLIB0039
            SecurityProtocol.TLS12 => SslProtocols.Tls12,
            SecurityProtocol.TLS13 => SslProtocols.Tls13,
            _ => throw new NotSupportedException(
                $"Protocol {protocol} is not supported"),
        };
    }
}
