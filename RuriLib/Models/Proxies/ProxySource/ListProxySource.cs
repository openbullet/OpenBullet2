using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies.ProxySources;

/// <summary>
/// Loads proxies from an in-memory list.
/// </summary>
public class ListProxySource : ProxySource
{
    private readonly Proxy[] proxies;

    /// <summary>
    /// Creates an in-memory proxy source.
    /// </summary>
    /// <param name="proxies">The proxies to expose.</param>
    public ListProxySource(IEnumerable<Proxy> proxies)
    {
        ArgumentNullException.ThrowIfNull(proxies);

        this.proxies = proxies.ToArray();
    }

    /// <inheritdoc />
    public override Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IEnumerable<Proxy>)proxies);
}
