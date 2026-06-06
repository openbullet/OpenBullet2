using RuriLib.Http.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Http.Curl;

/// <summary>
/// HTTP client backed by curl-impersonate.
/// </summary>
public sealed class CurlImpersonateHttpClient : IDisposable
{
    private readonly CurlImpersonateHandler handler;
    private readonly HttpClient client;
    private readonly CurlImpersonateHandlerOptions options;

    /// <summary>
    /// Creates a client with default curl-impersonate options.
    /// </summary>
    public CurlImpersonateHttpClient()
        : this(new CurlImpersonateHandlerOptions())
    {
    }

    /// <summary>
    /// Creates a client with the specified curl-impersonate options.
    /// </summary>
    public CurlImpersonateHttpClient(CurlImpersonateHandlerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this.options = options;
        handler = new CurlImpersonateHandler(options);
        client = new HttpClient(handler)
        {
            Timeout = Timeout.InfiniteTimeSpan
        };
    }

    /// <summary>
    /// Sends a request and returns a RuriLib HTTP response.
    /// </summary>
    public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        AddRequestCookies(request);

        var message = ToHttpRequestMessage(request);
        HttpResponseMessage? response = null;

        try
        {
            response = await client.SendAsync(message, HttpCompletionOption.ResponseContentRead, cancellationToken)
                .ConfigureAwait(false);

            var content = response.Content is null
                ? null
                : new ByteArrayContent(await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false));

            if (content is not null)
            {
                foreach (var header in response.Content!.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            UpdateRequestCookies(request);

            return new HttpResponse
            {
                Request = request,
                Version = response.Version,
                StatusCode = response.StatusCode,
                Content = content,
                Headers = response.Headers.ToDictionary(
                    h => h.Key,
                    h => string.Join(HeaderUsesCommaSeparator(h.Key) ? ", " : " ", h.Value),
                    StringComparer.InvariantCultureIgnoreCase)
            };
        }
        finally
        {
            response?.Dispose();
            message.Content = null;
            message.Dispose();
        }
    }

    private static HttpRequestMessage ToHttpRequestMessage(HttpRequest request)
    {
        if (request.Uri is null)
        {
            throw new InvalidOperationException("Request URI cannot be null.");
        }

        var message = new HttpRequestMessage
        {
            Method = request.Method,
            RequestUri = request.Uri,
            Version = request.Version,
            Content = request.Content
        };

        foreach (var header in request.Headers)
        {
            message.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Cookies.Count > 0 && !request.HeaderExists("Cookie", out _))
        {
            message.Headers.TryAddWithoutValidation("Cookie",
                string.Join("; ", request.Cookies.Select(c => $"{c.Key}={c.Value}")));
        }

        return message;
    }

    private void AddRequestCookies(HttpRequest request)
    {
        if (!options.UseCookies || request.Uri is null)
        {
            return;
        }

        foreach (var cookie in request.Cookies)
        {
            options.CookieContainer.Add(request.Uri, new Cookie(cookie.Key, cookie.Value));
        }
    }

    private void UpdateRequestCookies(HttpRequest request)
    {
        if (!options.UseCookies || request.Uri is null)
        {
            return;
        }

        foreach (Cookie cookie in options.CookieContainer.GetCookies(request.Uri))
        {
            request.Cookies[cookie.Name] = cookie.Value;
        }
    }

    private static bool HeaderUsesCommaSeparator(string name)
        => name.Equals("Accept", StringComparison.OrdinalIgnoreCase)
        || name.Equals("Accept-Encoding", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public void Dispose()
    {
        client.Dispose();
        handler.Dispose();
    }
}
