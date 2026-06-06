using RuriLib.Http.Curl.Internal;
using RuriLib.Http.Exceptions;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Http.Curl;

/// <summary>
/// <see cref="HttpMessageHandler"/> implementation backed by curl-impersonate.
/// </summary>
public sealed class CurlImpersonateHandler : HttpMessageHandler
{
    private readonly CurlImpersonateHandlerOptions options;
    private int disposed;

    /// <summary>
    /// Creates a handler with default options.
    /// </summary>
    public CurlImpersonateHandler()
        : this(new CurlImpersonateHandlerOptions())
    {
    }

    /// <summary>
    /// Creates a handler with the specified options.
    /// </summary>
    public CurlImpersonateHandler(CurlImpersonateHandlerOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
        => SendAsync(request, 0, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, int redirects,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref disposed) == 1, this);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.RequestUri);

        if (redirects > options.MaxNumberOfRedirects)
        {
            throw new RLHttpException("Maximum number of redirects exceeded");
        }

        using var context = new CurlRequestContext(cancellationToken, options.ReadResponseContent);
        using var transfer = new CurlEasyTransfer(context);
        await transfer.ConfigureAsync(request, options, cancellationToken).ConfigureAwait(false);

        var data = await Task.Run(() => transfer.Perform(cancellationToken), cancellationToken)
            .ConfigureAwait(false);

        var response = CurlResponseMessageBuilder.Build(data, request, options);
        StoreResponseCookies(response, request.RequestUri);

        try
        {
            if (ShouldRedirect(response))
            {
                var redirectUri = GetRedirectUri(response, request.RequestUri);
                var redirectRequest = await CreateRedirectRequestAsync(request, response.StatusCode, redirectUri,
                    cancellationToken).ConfigureAwait(false);

                response.Dispose();
                return await SendAsync(redirectRequest, redirects + 1, cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            response.Dispose();
            throw;
        }

        return response;
    }

    private bool ShouldRedirect(HttpResponseMessage response)
        => options.AllowAutoRedirect
        && (int)response.StatusCode / 100 == 3;

    private static Uri GetRedirectUri(HttpResponseMessage response, Uri baseUri)
    {
        var location = response.Headers.Location?.ToString();

        if (location is null && response.Headers.TryGetValues("Location", out var values))
        {
            location = values.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            throw new RLHttpException($"Status code was {(int)response.StatusCode} but no Location header received. " +
                                      "Disable auto redirect and try again.");
        }

        if (!Uri.TryCreate(location, UriKind.RelativeOrAbsolute, out var redirectUri))
        {
            throw new RLHttpException($"The Location header value '{location}' is not a valid URI");
        }

        return redirectUri.IsAbsoluteUri
            ? redirectUri
            : new Uri(baseUri, redirectUri);
    }

    private static async Task<HttpRequestMessage> CreateRedirectRequestAsync(HttpRequestMessage request,
        HttpStatusCode statusCode, Uri redirectUri, CancellationToken cancellationToken)
    {
        var redirected = new HttpRequestMessage
        {
            RequestUri = redirectUri,
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Origin", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            redirected.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        var keepVerb = statusCode is HttpStatusCode.TemporaryRedirect or HttpStatusCode.PermanentRedirect;
        redirected.Method = keepVerb ? request.Method : HttpMethod.Get;

        if (keepVerb && request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            redirected.Content = new ByteArrayContent(bytes);

            foreach (var header in request.Content.Headers)
            {
                redirected.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return redirected;
    }

    private void StoreResponseCookies(HttpResponseMessage response, Uri requestUri)
    {
        if (!options.UseCookies)
        {
            return;
        }

        foreach (var headerName in new[] { "Set-Cookie", "Set-Cookie2" })
        {
            if (!response.Headers.TryGetValues(headerName, out var values))
            {
                continue;
            }

            foreach (var value in values)
            {
                HttpResponseMessageBuilder.SetCookies(value, options.CookieContainer, requestUri);
            }
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Interlocked.Exchange(ref disposed, 1);
        }

        base.Dispose(disposing);
    }
}
