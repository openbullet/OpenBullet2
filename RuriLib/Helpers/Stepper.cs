using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    // TODO: Rework this with a better pattern (StepTokenSource and StepToken)
    // see PauseTokenSource and PauseToken for inspiration. This works but it's not very
    // good pattern-wise, it would be better with a publisher/consumer model.
    public class Stepper
    {
        private CancellationTokenSource waitCts;

        /// <summary>
        /// True if the stepper is waiting for a step.
        /// </summary>
        public bool IsWaiting => waitCts != null;

        public event EventHandler WaitingForStep;

        /// <summary>
        /// Asynchronously waits until the <see cref="Stepper.TryTakeStep"/> method is called,
        /// or the <paramref name="cancellationToken"/> is cancelled.
        /// </summary>
        public async Task WaitForStepAsync(CancellationToken cancellationToken = default)
        {
            // If the user cancelled the work, throw
            cancellationToken.ThrowIfCancellationRequested();

            waitCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(waitCts.Token, cancellationToken);

            try
            {
                WaitingForStep?.Invoke(this, EventArgs.Empty);

                // Wait indefinitely until the linked token is cancelled
                // (via the TryTakeStep() method or via the provided cancellationToken)
                await Task.Delay(-1, linkedCts.Token);
            }
            catch (TaskCanceledException)
            {
                // Dispose so we don't waste resources
                waitCts.Dispose();
                waitCts = null;

                // If it was the user to cancel, rethrow
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// Takes a step, returns true if the step was actually taken,
        /// false if the stepper was not waiting.
        /// </summary>
        public bool TryTakeStep()
        {
            // Cancel the wait (if the stepper is actually waiting)
            if (IsWaiting)
            {
                waitCts.Cancel();
                return true;
            }

            return false;
        }
    }
}
