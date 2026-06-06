using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers;

// TODO: Rework this with a better pattern (StepTokenSource and StepToken)
// see PauseTokenSource and PauseToken for inspiration. This works but it's not very
// good pattern-wise, it would be better with a publisher/consumer model.
/// <summary>
/// Coordinates one-step-at-a-time execution for debugging scenarios.
/// </summary>
public class Stepper
{
    private CancellationTokenSource? waitCts;

    /// <summary>
    /// True if the stepper is waiting for a step.
    /// </summary>
    public bool IsWaiting => waitCts is not null;

    /// <summary>
    /// Raised when the stepper begins waiting for a step.
    /// </summary>
    public event EventHandler? WaitingForStep;

    /// <summary>
    /// Asynchronously waits until the <see cref="TryTakeStep"/> method is called,
    /// or the <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task WaitForStepAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var cts = new CancellationTokenSource();
        waitCts = cts;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);

        try
        {
            WaitingForStep?.Invoke(this, EventArgs.Empty);

            // Wait indefinitely until the linked token is cancelled
            // by either TryTakeStep() or the provided cancellationToken.
            await Task.Delay(Timeout.InfiniteTimeSpan, linkedCts.Token);
        }
        catch (TaskCanceledException)
        {
            // If it was the user to cancel, rethrow as an OCE.
            cancellationToken.ThrowIfCancellationRequested();
        }
        finally
        {
            // Always clear and dispose the current CTS when the wait ends.
            var current = Interlocked.Exchange(ref waitCts, null);
            current?.Dispose();
        }
    }

    /// <summary>
    /// Takes a step, returns true if the step was actually taken,
    /// false if the stepper was not waiting.
    /// </summary>
    /// <returns><c>true</c> if a waiting step was released; otherwise <c>false</c>.</returns>
    public bool TryTakeStep()
    {
        var cts = waitCts;

        if (cts is null)
        {
            return false;
        }

        try
        {
            cts.Cancel();
            return true;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }
}
