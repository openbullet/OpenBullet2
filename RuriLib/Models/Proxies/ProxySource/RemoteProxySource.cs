using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies.ProxySources;

/// <summary>
/// Loads proxies from a remote HTTP endpoint.
/// </summary>
public class RemoteProxySource : ProxySource
{
    /// <summary>
    /// Gets or sets the remote URL.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Creates a remote proxy source.
    /// </summary>
    /// <param name="url">The URL to download proxies from.</param>
    public RemoteProxySource(string url)
    {
        ArgumentNullException.ThrowIfNull(url);

        Url = url;
    }

    /// <inheritdoc />
    public override async Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var client = new HttpClient();
        using var request = new HttpRequestMessage();

        request.RequestUri = new Uri(Url);
        request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var lines = content.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);

        return lines
            .Select(l => Proxy.TryParse(l, out var proxy, DefaultType, DefaultUsername, DefaultPassword) ? proxy : null)
            .OfType<Proxy>();
    }
}
