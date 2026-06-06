using RuriLib.Functions.Files;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Functions.Files;

public class FileLockerTests
{
    [Fact]
    public void GetHandle_SameFileName_ReturnsSameInstance()
    {
        var fileName = $"test-{Guid.NewGuid():N}.txt";

        var first = FileLocker.GetHandle(fileName);
        var second = FileLocker.GetHandle(fileName);

        Assert.Same(first, second);
    }

    [Fact]
    public void GetHandle_NullFileName_Throws()
        => Assert.Throws<ArgumentNullException>(() => FileLocker.GetHandle(null!));

    [Fact]
    public async Task EnterWriteLock_BlocksReadersUntilReleased()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var rwLock = new RWLock();
        await rwLock.EnterWriteLock(cancellationToken);

        var readTask = rwLock.EnterReadLock(cancellationToken);
        var completedTask = await Task.WhenAny(readTask, Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken));

        Assert.NotSame(readTask, completedTask);

        rwLock.ExitWriteLock();
        await readTask.WaitAsync(TimeSpan.FromSeconds(1), cancellationToken);
        rwLock.ExitReadLock();
    }

    [Fact]
    public async Task EnterReadLock_BlocksWritersUntilReleased()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var rwLock = new RWLock();
        await rwLock.EnterReadLock(cancellationToken);

        var writeTask = rwLock.EnterWriteLock(cancellationToken);
        var completedTask = await Task.WhenAny(writeTask, Task.Delay(TimeSpan.FromMilliseconds(50), cancellationToken));

        Assert.NotSame(writeTask, completedTask);

        rwLock.ExitReadLock();
        await writeTask.WaitAsync(TimeSpan.FromSeconds(1), cancellationToken);
        rwLock.ExitWriteLock();
    }
}
