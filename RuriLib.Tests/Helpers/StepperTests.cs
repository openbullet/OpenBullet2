using RuriLib.Helpers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace RuriLib.Tests.Helpers;

public class StepperTests
{
    private static CancellationToken TestCancellationToken => TestContext.Current.CancellationToken;

    [Fact]
    public async Task WaitForStepAsync_TakeStep_CompletesAndResetsWaitingState()
    {
        var stepper = new Stepper();
        // Use the event to deterministically detect when the stepper starts waiting.
        var waiting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        stepper.WaitingForStep += (_, _) => waiting.TrySetResult();

        var task = stepper.WaitForStepAsync(TestCancellationToken);
        await waiting.Task.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);

        Assert.True(stepper.IsWaiting);
        Assert.True(stepper.TryTakeStep());

        await task.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
        Assert.False(stepper.IsWaiting);
    }

    [Fact]
    public async Task WaitForStepAsync_Cancelled_ThrowsAndResetsWaitingState()
    {
        var stepper = new Stepper();
        // Use the event to avoid racing the cancellation against stepper initialization.
        var waiting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        stepper.WaitingForStep += (_, _) => waiting.TrySetResult();

        using var cts = new CancellationTokenSource();
        var task = stepper.WaitForStepAsync(cts.Token);

        await waiting.Task.WaitAsync(TimeSpan.FromSeconds(5), TestCancellationToken);
        Assert.True(stepper.IsWaiting);

        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
        Assert.False(stepper.IsWaiting);
    }

    [Fact]
    public void TryTakeStep_WhenNotWaiting_ReturnsFalse()
    {
        var stepper = new Stepper();

        Assert.False(stepper.TryTakeStep());
    }
}
