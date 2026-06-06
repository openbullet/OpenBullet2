using System.Collections.Generic;
using RuriLib.Http.Curl;

namespace RuriLib.Functions.Http.Options;

/// <summary>
/// Base options for an HTTP request.
/// </summary>
public class HttpRequestOptions
{
    /// <summary>
    /// Gets or sets the request URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTTP method.
    /// </summary>
    public HttpMethod Method { get; set; } = HttpMethod.GET;

    /// <summary>
    /// Gets or sets whether redirects are followed automatically.
    /// </summary>
    public bool AutoRedirect { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum number of redirects.
    /// </summary>
    public int MaxNumberOfRedirects { get; set; } = 8;

    /// <summary>
    /// Gets or sets whether the absolute URI should be used in the first request line.
    /// </summary>
    public bool AbsoluteUriInFirstLine { get; set; }

    /// <summary>
    /// Gets or sets the HTTP implementation to use.
    /// </summary>
    public HttpLibrary HttpLibrary { get; set; } = HttpLibrary.RuriLibHttp;

    /// <summary>
    /// Gets or sets the TLS/SSL protocol selection.
    /// </summary>
    public SecurityProtocol SecurityProtocol { get; set; } = SecurityProtocol.SystemDefault;

    /// <summary>
    /// Gets or sets whether TLS certificate validity checks should be bypassed.
    /// </summary>
    public bool IgnoreCertificateValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom cookies to send.
    /// </summary>
    public Dictionary<string, string> CustomCookies { get; set; } = new();

    /// <summary>
    /// Gets or sets the custom headers to send.
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();

    /// <summary>
    /// Gets or sets the timeout in milliseconds.
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 10000;

    /// <summary>
    /// Gets or sets the HTTP version string.
    /// </summary>
    public string HttpVersion { get; set; } = "1.1";

    /// <summary>
    /// Gets or sets whether to use custom cipher suites.
    /// </summary>
    public bool UseCustomCipherSuites { get; set; }

    /// <summary>
    /// Gets or sets the custom cipher suite names.
    /// </summary>
    public List<string> CustomCipherSuites { get; set; } = new();

    /// <summary>
    /// Gets or sets the code pages encoding name.
    /// </summary>
    public string CodePagesEncoding { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether content should always be sent.
    /// </summary>
    public bool AlwaysSendContent { get; set; }

    /// <summary>
    /// Gets or sets whether HTML should be decoded.
    /// </summary>
    public bool DecodeHtml { get; set; }

    /// <summary>
    /// Gets or sets whether response content should be read.
    /// </summary>
    public bool ReadResponseContent { get; set; } = true;

    /// <summary>
    /// Gets or sets the browser profile used when <see cref="HttpLibrary"/> is
    /// <see cref="HttpLibrary.CurlImpersonate"/>.
    /// </summary>
    public CurlImpersonateBrowserProfile CurlImpersonateBrowserProfile { get; set; } =
        CurlImpersonateBrowserProfile.Chrome142;

    /// <summary>
    /// Gets or sets whether curl-impersonate should send its browser-default
    /// headers when <see cref="HttpLibrary"/> is <see cref="HttpLibrary.CurlImpersonate"/>.
    /// </summary>
    public bool CurlUseBrowserHeaders { get; set; } = true;
}

/// <summary>
/// Identifies the HTTP implementation to use.
/// </summary>
public enum HttpLibrary
{
    /// <summary>
    /// Use the built-in <c>System.Net.Http</c> stack.
    /// </summary>
    SystemNet,

    /// <summary>
    /// Use the custom RuriLib HTTP stack.
    /// </summary>
    RuriLibHttp,

    /// <summary>
    /// Use curl-impersonate for browser TLS and HTTP fingerprint impersonation.
    /// </summary>
    CurlImpersonate
}
