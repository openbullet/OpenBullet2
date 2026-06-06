using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Models.Proxies;

/// <summary>
/// Base class for sources that provide proxies.
/// </summary>
public abstract class ProxySource : IDisposable
{
    /// <summary>
    /// Gets or sets the default proxy type to assume for parsed entries.
    /// </summary>
    public ProxyType DefaultType { get; set; } = ProxyType.Http;

    /// <summary>
    /// Gets or sets the default username to apply to parsed entries.
    /// </summary>
    public string DefaultUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the default password to apply to parsed entries.
    /// </summary>
    public string DefaultPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owning user identifier.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Shared random generator used by sources that need randomized selection.
    /// </summary>
    protected readonly Random random = new();

    /// <summary>
    /// Retrieves all proxies from the source.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that returns the parsed proxies.</returns>
    public virtual Task<IEnumerable<Proxy>> GetAllAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    /// Releases resources owned by the proxy source.
    /// </summary>
    public virtual void Dispose()
    {
    }
}
