using RuriLib.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class AsyncLockerTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Acquire_SameKey_BlocksUntilRelease()
    {
        using var locker = new AsyncLocker();

        await locker.Acquire("key", TestCancellationToken);
        var secondAcquire = locker.Acquire("key", TestCancellationToken);

        Assert.False(secondAcquire.IsCompleted);

        locker.Release("key");
        await secondAcquire.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
    }

    [Fact]
    public async Task Acquire_TypeAndMethod_UsesCombinedKey()
    {
        using var locker = new AsyncLocker();

        await locker.Acquire(typeof(AsyncLockerTests), nameof(Acquire_TypeAndMethod_UsesCombinedKey), TestCancellationToken);
        var secondAcquire = locker.Acquire(typeof(AsyncLockerTests), nameof(Acquire_TypeAndMethod_UsesCombinedKey), TestCancellationToken);

        Assert.False(secondAcquire.IsCompleted);

        locker.Release(typeof(AsyncLockerTests), nameof(Acquire_TypeAndMethod_UsesCombinedKey));
        await secondAcquire.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
    }

    [Fact]
    public void Release_MissingKey_ThrowsInvalidOperationException()
    {
        using var locker = new AsyncLocker();

        Assert.Throws<InvalidOperationException>(() => locker.Release("missing"));
    }
}
