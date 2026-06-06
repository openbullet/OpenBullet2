using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using RuriLib.Http.Curl;

namespace RuriLib.Functions.Http;

/// <summary>
/// Configures shared HTTP client behavior.
/// </summary>
public class HttpOptions
{
    /// <summary>
    /// Gets or sets the connection timeout.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets the read/write timeout.
    /// </summary>
    public TimeSpan ReadWriteTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets whether redirects are followed automatically.
    /// </summary>
    public bool AutoRedirect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of redirects to follow.
    /// </summary>
    public int MaxNumberOfRedirects { get; set; } = 8;

    /// <summary>
    /// Gets or sets whether response content should be read.
    /// </summary>
    public bool ReadResponseContent { get; set; } = true;

    /// <summary>
    /// Gets or sets the TLS/SSL protocol selection.
    /// </summary>
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SystemDefault;

    /// <summary>
    /// Gets or sets whether TLS certificate validity checks should be bypassed.
    /// </summary>
    public bool IgnoreCertificateValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to use the custom cipher suite list.
    /// </summary>
    public bool UseCustomCipherSuites { get; set; }

    /// <summary>
    /// Gets or sets the certificate revocation mode.
    /// </summary>
    public X509RevocationMode CertRevocationMode { get; set; } = X509RevocationMode.NoCheck;

    /// <summary>
    /// Gets or sets the custom cipher suites to use when enabled.
    /// </summary>
    public TlsCipherSuite[] CustomCipherSuites { get; set; } =
    [
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
    ];

    /// <summary>
    /// Gets or sets the browser profile used by the curl-impersonate HTTP library.
    /// </summary>
    public CurlImpersonateBrowserProfile CurlImpersonateBrowserProfile { get; set; } =
        CurlImpersonateBrowserProfile.Chrome142;

    /// <summary>
    /// Gets or sets whether curl-impersonate should send its browser-default headers.
    /// </summary>
    public bool CurlUseBrowserHeaders { get; set; } = true;
}
