using System;
using System.Net;

namespace RuriLib.Http.Curl;

/// <summary>
/// Options for <see cref="CurlImpersonateHandler"/>.
/// </summary>
public sealed class CurlImpersonateHandlerOptions
{
    /// <summary>
    /// Browser profile to impersonate.
    /// </summary>
    public CurlImpersonateBrowserProfile BrowserProfile { get; set; } = CurlImpersonateBrowserProfile.Chrome142;

    /// <summary>
    /// Whether curl-impersonate should add the default browser headers for the selected profile.
    /// </summary>
    /// <remarks>
    /// When enabled, browser-managed headers supplied on the request, such as
    /// <c>User-Agent</c>, <c>Accept</c> and <c>Accept-Language</c>, are ignored so they do
    /// not override curl-impersonate's header order.
    /// </remarks>
    public bool UseBrowserHeaders { get; set; } = true;

    /// <summary>
    /// Whether redirects are followed by the handler.
    /// </summary>
    public bool AllowAutoRedirect { get; set; } = true;

    /// <summary>
    /// Maximum number of redirects to follow.
    /// </summary>
    public int MaxNumberOfRedirects { get; set; } = 8;

    /// <summary>
    /// Whether the response body should be read.
    /// </summary>
    public bool ReadResponseContent { get; set; } = true;

    /// <summary>
    /// Whether response content should be decompressed when a supported
    /// <c>Content-Encoding</c> header is present.
    /// </summary>
    public bool AutomaticDecompression { get; set; } = true;

    /// <summary>
    /// Whether TLS certificate checks should be bypassed.
    /// </summary>
    public bool IgnoreCertificateValidation { get; set; } = true;

    /// <summary>
    /// Connection timeout.
    /// </summary>
    public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Optional transfer timeout. Use <c>Timeout.InfiniteTimeSpan</c> to rely only
    /// on cancellation tokens.
    /// </summary>
    public TimeSpan Timeout { get; set; } = System.Threading.Timeout.InfiniteTimeSpan;

    /// <summary>
    /// Optional proxy URI, for example <c>http://127.0.0.1:8080</c> or <c>socks5://127.0.0.1:1080</c>.
    /// </summary>
    public Uri? ProxyUri { get; set; }

    /// <summary>
    /// Optional proxy credentials.
    /// </summary>
    public NetworkCredential? ProxyCredentials { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the handler uses the
    /// <see cref="CookieContainer"/> property.
    /// </summary>
    public bool UseCookies { get; set; } = true;

    /// <summary>
    /// Cookie container used to store server cookies and send them on requests.
    /// </summary>
    public CookieContainer CookieContainer { get; set; } = new();
}
