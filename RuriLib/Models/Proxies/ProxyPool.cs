using RuriLib.Extensions;
using RuriLib.Helpers;
using RuriLib.Models.Proxies.ProxySources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace RuriLib.Models.Proxies;

/// <summary>
/// Manages a shared pool of proxies loaded from one or more sources.
/// </summary>
public class ProxyPool : IDisposable
{
    /// <summary>All the proxies currently in the pool as a stable snapshot.</summary>
    public IEnumerable<Proxy> Proxies
    {
        get
        {
            lock (proxiesLock)
            {
                return proxies.ToArray();
            }
        }
    }

    /// <summary>Checks if all proxies are banned.</summary>
    public bool AllBanned
    {
        get
        {
            lock (proxiesLock)
            {
                return proxies.All(p => p.ProxyStatus is ProxyStatus.Bad or ProxyStatus.Banned);
            }
        }
    }

    private List<Proxy> proxies = [];
    private readonly object proxiesLock = new();
    private bool isReloadingProxies;
    private readonly List<ProxySource> sources;
    private readonly ProxyPoolOptions options;

    private readonly int minBackoff = 5000;
    private readonly int maxReloadTries = 10;
    private AsyncLocker? asyncLocker;
    private readonly ILogger<ProxyPool> logger;

    /// <summary>
    /// Initializes the proxy pool given the proxy sources.
    /// </summary>
    /// <param name="sources">The proxy sources to load from.</param>
    /// <param name="options">Optional pool behavior settings.</param>
    /// <param name="logger">The optional logger.</param>
    public ProxyPool(IEnumerable<ProxySource> sources, ProxyPoolOptions? options = null,
        ILogger<ProxyPool>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(sources);

        this.sources = sources.ToList();
        this.options = options ?? new ProxyPoolOptions();
        this.logger = logger ?? NullLogger<ProxyPool>.Instance;
        asyncLocker = new();
    }

    /// <summary>
    /// Sets all the BANNED proxies status to AVAILABLE and resets their Uses.
    /// </summary>
    /// <param name="minimumBanTime">The minimum ban duration that must have elapsed before a proxy can be unbanned.</param>
    public void UnbanAll(TimeSpan minimumBanTime)
    {
        var now = DateTime.Now;
        lock (proxiesLock)
        {
            proxies.Where(p => now > p.LastBanned + minimumBanTime).ToList().ForEach(p =>
            {
                if (p.ProxyStatus == ProxyStatus.Banned)
                {
                    p.ProxyStatus = ProxyStatus.Available;
                    p.BeingUsedBy = 0;
                    p.TotalUses = 0;
                }
            });
        }
    }

    /// <summary>
    /// Tries to return the first available proxy from the list
    /// (or even a Busy one if <paramref name="evenBusy"/> is true).
    /// If <paramref name="maxUses"/> is not 0, the pool will try to
    /// return a proxy that was used less than <paramref name="maxUses"/> times.
    /// Returns null if no proxy matching the required parameters was found.
    /// </summary>
    /// <param name="evenBusy">Whether busy proxies are also considered valid candidates.</param>
    /// <param name="maxUses">The maximum number of times a proxy can have been used, or 0 for no limit.</param>
    /// <returns>The selected proxy, or <see langword="null"/> if none matched.</returns>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)] //hot path
    public Proxy? GetProxy(bool evenBusy = false, int maxUses = 0)
    {
        lock (proxiesLock)
        {
            for (var i = 0; i < proxies.Count; i++)
            {
                var px = proxies[i];
                if (evenBusy)
                {
                    if (px.ProxyStatus is ProxyStatus.Available or ProxyStatus.Busy)
                    {
                        if (maxUses > 0)
                        {
                            if (px.TotalUses < maxUses)
                            {
                                px.BeingUsedBy++;
                                px.ProxyStatus = ProxyStatus.Busy;
                                return px;
                            }
                        }
                        else
                        {
                            px.BeingUsedBy++;
                            px.ProxyStatus = ProxyStatus.Busy;
                            return px;
                        }
                    }
                }
                else if (px.ProxyStatus == ProxyStatus.Available)
                {
                    if (maxUses > 0)
                    {
                        if (px.TotalUses < maxUses)
                        {
                            px.BeingUsedBy++;
                            px.ProxyStatus = ProxyStatus.Busy;
                            return px;
                        }
                    }
                    else
                    {
                        px.BeingUsedBy++;
                        px.ProxyStatus = ProxyStatus.Busy;
                        return px;
                    }
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Releases a proxy that was being used, optionally banning it.
    /// </summary>
    /// <param name="proxy">The proxy to release.</param>
    /// <param name="ban">Whether the proxy should be marked as banned.</param>
    public void ReleaseProxy(Proxy proxy, bool ban = false)
        => ReleaseProxy(proxy, ban ? ProxyStatus.Banned : ProxyStatus.Available);

    /// <summary>
    /// Releases a proxy that was being used and assigns its resulting status.
    /// </summary>
    /// <param name="proxy">The proxy to release.</param>
    /// <param name="status">The status to assign after the proxy is released.</param>
    public void ReleaseProxy(Proxy proxy, ProxyStatus status)
    {
        ArgumentNullException.ThrowIfNull(proxy);

        lock (proxiesLock)
        {
            proxy.TotalUses++;
            proxy.BeingUsedBy--;

            if (status == ProxyStatus.Busy)
            {
                throw new ArgumentException("Cannot release a proxy as busy", nameof(status));
            }

            if (status == ProxyStatus.Banned)
            {
                proxy.ProxyStatus = ProxyStatus.Banned;
                proxy.LastBanned = DateTime.Now;
            }
            else
            {
                proxy.ProxyStatus = status;
            }
        }
    }

    /// <summary>
    /// Removes duplicates.
    /// </summary>
    public void RemoveDuplicates()
    {
        lock (proxiesLock)
        {
            proxies = proxies.Distinct(ProxyIdentityComparer.Instance).ToList();
        }
    }

    /// <summary>
    /// Shuffles proxies.
    /// </summary>
    public void Shuffle()
    {
        lock (proxiesLock)
        {
            proxies.Shuffle();
        }
    }

    /// <summary>
    /// Reloads all proxies in the pool from the provided sources.
    /// </summary>
    /// <param name="shuffle">Whether the loaded proxies should be shuffled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the reload process finishes.</returns>
    public async Task ReloadAllAsync(bool shuffle = true, CancellationToken cancellationToken = default)
        => await ReloadAllAsync(shuffle, retryOnEmpty: true, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Reloads all proxies in the pool from the provided sources once, without retry backoff if no proxies are found.
    /// </summary>
    /// <param name="shuffle">Whether the loaded proxies should be shuffled.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if at least one source returned proxies.</returns>
    public Task<bool> ReloadAllOnceAsync(bool shuffle = true, CancellationToken cancellationToken = default)
        => ReloadAllAsync(shuffle, retryOnEmpty: false, cancellationToken);

    private async Task<bool> ReloadAllAsync(bool shuffle, bool retryOnEmpty, CancellationToken cancellationToken)
    {
        if (isReloadingProxies)
        {
            return false;
        }

        var locker = asyncLocker ?? throw new ObjectDisposedException(nameof(ProxyPool));
        var reloadLockAcquired = false;

        try
        {
            isReloadingProxies = true;
            await locker.Acquire(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync), cancellationToken).ConfigureAwait(false);
            reloadLockAcquired = true;
            var currentTry = 0;
            var currentBackoff = minBackoff;

            // For a maximum of 'maxReloadTries' times
            while (currentTry < maxReloadTries)
            {
                // Try to reload proxies from sources
                if (await TryReloadAllAsync(shuffle, cancellationToken).ConfigureAwait(false))
                {
                    return true;
                }

                if (!retryOnEmpty)
                {
                    return false;
                }

                // If it fails to fetch at least 1 proxy, backoff by an increasing amount (e.g. to prevent rate limiting)
                logger.LogWarning("Failed to reload proxies, no proxies found. Waiting {BackoffMs} ms before retrying", currentBackoff);
                await Task.Delay(currentBackoff, cancellationToken).ConfigureAwait(false);

                currentTry++;
                currentBackoff *= 2;
            }

            return false;
        }
        finally
        {
            isReloadingProxies = false;

            if (reloadLockAcquired)
            {
                locker.Release(typeof(ProxyPool), nameof(ProxyPool.ReloadAllAsync));
            }
        }
    }

    private async Task<bool> TryReloadAllAsync(bool shuffle = true, CancellationToken cancellationToken = default)
    {
        var tasks = sources.Select(async source =>
        {
            try
            {
                return await source.GetAllAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                switch (source)
                {
                    case FileProxySource x:
                        logger.LogWarning(ex, "Could not reload proxies from file source {FileName}", x.FileName);
                        break;

                    case RemoteProxySource x:
                        logger.LogWarning(ex, "Could not reload proxies from remote source {Url}", x.Url);
                        break;

                    default:
                        logger.LogWarning(ex, "Could not reload proxies from an unknown source");
                        break;
                }

                return Enumerable.Empty<Proxy>();
            }
        });

        var results = (await Task.WhenAll(tasks).ConfigureAwait(false)).SelectMany(r => r).ToList();

        // If no results, return false to trigger the backoff mechanism (do not remove existing proxies)
        if (results.Count == 0)
        {
            return false;
        }

        lock (proxiesLock)
        {
            proxies = results
                .Where(p => options.AllowedTypes.Contains(p.Type)) // Filter by allowed types
                .Distinct(ProxyIdentityComparer.Instance)
                .ToList();

            if (shuffle)
            {
                proxies.Shuffle();
            }
        }

        return true;
    }

    /// <summary>
    /// Releases resources owned by the proxy pool.
    /// </summary>
    public void Dispose()
    {
        if (asyncLocker is not null)
        {
            try
            {
                asyncLocker.Dispose();
                asyncLocker = null;
            }
            catch
            {
                // ignored
            }
        }
    }

    private sealed class ProxyIdentityComparer : IEqualityComparer<Proxy>
    {
        public static ProxyIdentityComparer Instance { get; } = new();

        public bool Equals(Proxy? x, Proxy? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null || y is null)
            {
                return false;
            }

            return x.Port == y.Port
                && x.Type == y.Type
                && x.Host == y.Host
                && x.Username == y.Username
                && x.Password == y.Password;
        }

        public int GetHashCode(Proxy obj)
            => HashCode.Combine(obj.Host, obj.Port, obj.Type, obj.Username, obj.Password);
    }
}
