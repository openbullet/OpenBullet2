using Newtonsoft.Json;
using RuriLib.Models.Settings;
using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Functions.Captchas;

/// <summary>
/// Caches captcha balance checks for a short time to reduce rate limiting.
/// </summary>
public static class CaptchaBalanceCache
{
    private sealed record CacheEntry(decimal Balance, DateTimeOffset ExpiresAt);

    private static readonly ConcurrentDictionary<string, CacheEntry> cache = new();
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> locks = new();
    private static readonly TimeSpan defaultCacheLifetime = TimeSpan.FromSeconds(10);

    internal static Func<DateTimeOffset> UtcNowProvider { get; set; } = static () => DateTimeOffset.UtcNow;
    internal static TimeSpan CacheLifetime { get; set; } = defaultCacheLifetime;

    /// <summary>
    /// Gets the current account balance, reusing a recent result when available.
    /// </summary>
    public static Task<decimal> GetBalanceAsync(CaptchaSettings settings,
        CancellationToken cancellationToken = default)
        => GetBalanceAsync(settings, CaptchaServiceFactory.GetService(settings).GetBalanceAsync, false,
            cancellationToken);

    /// <summary>
    /// Refreshes the cached balance with a new request to the captcha service.
    /// </summary>
    public static Task<decimal> RefreshBalanceAsync(CaptchaSettings settings,
        CancellationToken cancellationToken = default)
        => GetBalanceAsync(settings, CaptchaServiceFactory.GetService(settings).GetBalanceAsync, true,
            cancellationToken);

    internal static async Task<decimal> GetBalanceAsync(CaptchaSettings settings,
        Func<CancellationToken, Task<decimal>> balanceFactory,
        CancellationToken cancellationToken = default)
        => await GetBalanceAsync(settings, balanceFactory, false, cancellationToken).ConfigureAwait(false);

    internal static async Task<decimal> RefreshBalanceAsync(CaptchaSettings settings,
        Func<CancellationToken, Task<decimal>> balanceFactory,
        CancellationToken cancellationToken = default)
        => await GetBalanceAsync(settings, balanceFactory, true, cancellationToken).ConfigureAwait(false);

    private static async Task<decimal> GetBalanceAsync(CaptchaSettings settings,
        Func<CancellationToken, Task<decimal>> balanceFactory,
        bool forceRefresh,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GetCacheKey(settings);

        if (!forceRefresh && TryGetFreshBalance(cacheKey, out var cachedBalance))
        {
            return cachedBalance;
        }

        var semaphore = locks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            if (!forceRefresh && TryGetFreshBalance(cacheKey, out cachedBalance))
            {
                return cachedBalance;
            }

            try
            {
                var balance = await balanceFactory(cancellationToken).ConfigureAwait(false);
                cache[cacheKey] = new CacheEntry(balance, UtcNowProvider().Add(CacheLifetime));
                return balance;
            }
            catch (Exception ex) when (!cancellationToken.IsCancellationRequested
                && !forceRefresh
                && IsRateLimitException(ex)
                && cache.TryGetValue(cacheKey, out var staleEntry))
            {
                return staleEntry.Balance;
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    internal static void ResetForTests()
    {
        cache.Clear();
        locks.Clear();
        CacheLifetime = defaultCacheLifetime;
        UtcNowProvider = static () => DateTimeOffset.UtcNow;
    }

    private static bool TryGetFreshBalance(string cacheKey, out decimal balance)
    {
        if (cache.TryGetValue(cacheKey, out var entry) && entry.ExpiresAt > UtcNowProvider())
        {
            balance = entry.Balance;
            return true;
        }

        balance = default;
        return false;
    }

    private static bool IsRateLimitException(Exception exception)
    {
        var message = exception.Message;

        return message.Contains("429", StringComparison.OrdinalIgnoreCase)
            || message.Contains("rate limit", StringComparison.OrdinalIgnoreCase)
            || message.Contains("too many requests", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetCacheKey(CaptchaSettings settings)
    {
        var serialized = JsonConvert.SerializeObject(settings);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(serialized));
        return Convert.ToHexString(hash);
    }
}
