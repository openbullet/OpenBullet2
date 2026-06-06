using RuriLib.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class PauseTokenSourceTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task PauseAsync_ConsumerAcknowledgesPause_MarksSourceAsPaused()
    {
        var pts = new PauseTokenSource();

        var pauseTask = pts.PauseAsync(TestCancellationToken);
        Assert.False(pauseTask.IsCompleted);

        // The consumer confirms the pause request before the source can enter the paused state.
        var pausedConsumerTask = pts.Token.PauseIfRequestedAsync(TestCancellationToken);
        await pauseTask.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);

        Assert.True(await pts.IsPausedAsync(TestCancellationToken));
        Assert.False(pausedConsumerTask.IsCompleted);

        await pts.ResumeAsync(TestCancellationToken);
        await pausedConsumerTask.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
    }

    [Fact]
    public async Task PauseIfRequestedAsync_PausedThenResumed_WaitsUntilResume()
    {
        var pts = new PauseTokenSource();
        var pauseTask = pts.PauseAsync(TestCancellationToken);
        var pausedConsumerTask = pts.Token.PauseIfRequestedAsync(TestCancellationToken);

        await pauseTask.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
        Assert.True(await pts.IsPausedAsync(TestCancellationToken));
        Assert.False(pausedConsumerTask.IsCompleted);

        await pts.ResumeAsync(TestCancellationToken);
        await pausedConsumerTask.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
        Assert.False(await pts.IsPausedAsync(TestCancellationToken));
    }

    [Fact]
    public async Task PauseIfRequestedAsync_WhenNotPaused_CompletesImmediately()
    {
        var pts = new PauseTokenSource();

        await pts.Token.PauseIfRequestedAsync(TestCancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);

        Assert.False(await pts.IsPausedAsync(TestCancellationToken));
    }

    [Fact]
    public async Task ResumeAsync_WhenNotPaused_CompletesWithoutChangingState()
    {
        var pts = new PauseTokenSource();

        await pts.ResumeAsync(TestCancellationToken).WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);

        Assert.False(await pts.IsPausedAsync(TestCancellationToken));
    }
}
