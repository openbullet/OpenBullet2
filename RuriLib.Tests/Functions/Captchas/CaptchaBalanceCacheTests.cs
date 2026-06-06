using RuriLib.Functions.Captchas;
using RuriLib.Models.Settings;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Captchas;

public class CaptchaBalanceCacheTests : IDisposable
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    public void Dispose() => CaptchaBalanceCache.ResetForTests();

    [Fact]
    public async Task GetBalanceAsync_WithinCacheLifetime_ReturnsCachedValue()
    {
        var settings = CreateSettings();
        var calls = 0;

        async Task<decimal> GetBalance(CancellationToken cancellationToken)
        {
            calls++;
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            return 12.34m;
        }

        var first = await CaptchaBalanceCache.GetBalanceAsync(settings, GetBalance, TestCancellationToken);
        var second = await CaptchaBalanceCache.GetBalanceAsync(settings, GetBalance, TestCancellationToken);

        Assert.Equal(12.34m, first);
        Assert.Equal(first, second);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetBalanceAsync_AfterCacheLifetime_RefreshesBalance()
    {
        var settings = CreateSettings();
        var now = new DateTimeOffset(2026, 05, 10, 12, 0, 0, TimeSpan.Zero);
        var calls = 0;

        CaptchaBalanceCache.UtcNowProvider = () => now;

        async Task<decimal> GetBalance(CancellationToken cancellationToken)
        {
            calls++;
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            return calls;
        }

        var first = await CaptchaBalanceCache.GetBalanceAsync(settings, GetBalance, TestCancellationToken);
        now = now.Add(CaptchaBalanceCache.CacheLifetime).Add(TimeSpan.FromSeconds(1));
        var second = await CaptchaBalanceCache.GetBalanceAsync(settings, GetBalance, TestCancellationToken);

        Assert.Equal(1m, first);
        Assert.Equal(2m, second);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task RefreshBalanceAsync_IgnoresFreshCache()
    {
        var settings = CreateSettings();
        var calls = 0;

        async Task<decimal> GetBalance(CancellationToken cancellationToken)
        {
            calls++;
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            return calls;
        }

        var first = await CaptchaBalanceCache.GetBalanceAsync(settings, GetBalance, TestCancellationToken);
        var second = await CaptchaBalanceCache.RefreshBalanceAsync(settings, GetBalance, TestCancellationToken);

        Assert.Equal(1m, first);
        Assert.Equal(2m, second);
        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task GetBalanceAsync_ConcurrentRequests_ShareSingleInFlightCall()
    {
        var settings = CreateSettings();
        var calls = 0;
        var tcs = new TaskCompletionSource<decimal>(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<decimal> GetBalance(CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref calls);
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            return tcs.Task;
        }

        var firstTask = CaptchaBalanceCache.GetBalanceAsync(settings, GetBalance, TestCancellationToken);
        var secondTask = CaptchaBalanceCache.GetBalanceAsync(settings, GetBalance, TestCancellationToken);

        await Task.Delay(100, TestCancellationToken);

        Assert.Equal(1, Volatile.Read(ref calls));

        tcs.SetResult(9.87m);
        var results = await Task.WhenAll(firstTask, secondTask);

        Assert.All(results, balance => Assert.Equal(9.87m, balance));
    }

    [Fact]
    public async Task GetBalanceAsync_RateLimitedAfterExpiry_ReturnsStaleBalance()
    {
        var settings = CreateSettings();
        var now = new DateTimeOffset(2026, 05, 10, 12, 0, 0, TimeSpan.Zero);

        CaptchaBalanceCache.UtcNowProvider = () => now;

        var first = await CaptchaBalanceCache.GetBalanceAsync(settings,
            _ => Task.FromResult(3.21m), TestCancellationToken);

        now = now.Add(CaptchaBalanceCache.CacheLifetime).Add(TimeSpan.FromSeconds(1));

        var second = await CaptchaBalanceCache.GetBalanceAsync(settings,
            _ => throw new InvalidOperationException("429 Too Many Requests"), TestCancellationToken);

        Assert.Equal(3.21m, first);
        Assert.Equal(first, second);
    }

    [Fact]
    public async Task GetBalanceAsync_NonRateLimitFailure_DoesNotUseStaleBalance()
    {
        var settings = CreateSettings();
        var now = new DateTimeOffset(2026, 05, 10, 12, 0, 0, TimeSpan.Zero);

        CaptchaBalanceCache.UtcNowProvider = () => now;

        await CaptchaBalanceCache.GetBalanceAsync(settings,
            _ => Task.FromResult(7.89m), TestCancellationToken);

        now = now.Add(CaptchaBalanceCache.CacheLifetime).Add(TimeSpan.FromSeconds(1));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CaptchaBalanceCache.GetBalanceAsync(settings,
                _ => throw new InvalidOperationException("Invalid API key"),
                TestCancellationToken));

        Assert.Equal("Invalid API key", exception.Message);
    }

    private static CaptchaSettings CreateSettings() => new()
    {
        CurrentService = CaptchaServiceType.TwoCaptcha,
        TwoCaptchaApiKey = "test-key"
    };
}
