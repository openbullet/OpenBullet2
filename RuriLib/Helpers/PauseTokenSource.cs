using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers;

// PauseTokenSource. Code from https://stackoverflow.com/questions/19613444/a-pattern-to-pause-resume-an-async-task
/// <summary>
/// Coordinates pause and resume requests between a producer and a consumer.
/// </summary>
public class PauseTokenSource
{
    private bool paused;
    private bool pauseRequested;

    private TaskCompletionSource<bool>? resumeRequestTcs;
    private TaskCompletionSource<bool>? pauseConfirmationTcs;

    private readonly SemaphoreSlim stateAsyncLock = new(1);
    private readonly SemaphoreSlim pauseRequestAsyncLock = new(1);

    /// <summary>
    /// Gets the consumer-side pause token.
    /// </summary>
    public PauseToken Token => new(this);

    /// <summary>
    /// Returns whether the source is currently paused.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns><c>true</c> if paused; otherwise <c>false</c>.</returns>
    public async Task<bool> IsPausedAsync(CancellationToken token = default)
    {
        await stateAsyncLock.WaitAsync(token);

        try
        {
            return paused;
        }
        finally
        {
            stateAsyncLock.Release();
        }
    }

    /// <summary>
    /// Resumes the paused consumer if one is waiting.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    public async Task ResumeAsync(CancellationToken token = default)
    {
        await stateAsyncLock.WaitAsync(token);

        try
        {
            if (!paused)
            {
                return;
            }

            await pauseRequestAsyncLock.WaitAsync(token);

            try
            {
                var pendingResumeRequest = resumeRequestTcs;
                paused = false;
                pauseRequested = false;
                resumeRequestTcs = null;
                pauseConfirmationTcs = null;
                pendingResumeRequest?.TrySetResult(true);
            }
            finally
            {
                pauseRequestAsyncLock.Release();
            }
        }
        finally
        {
            stateAsyncLock.Release();
        }
    }

    /// <summary>
    /// Requests the consumer to pause and waits for confirmation.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    public async Task PauseAsync(CancellationToken token = default)
    {
        await stateAsyncLock.WaitAsync(token);

        try
        {
            if (paused)
            {
                return;
            }

            Task? pauseConfirmationTask = null;
            await pauseRequestAsyncLock.WaitAsync(token);

            try
            {
                pauseRequested = true;
                resumeRequestTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                pauseConfirmationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                pauseConfirmationTask = WaitForPauseConfirmationAsync(token);
            }
            finally
            {
                pauseRequestAsyncLock.Release();
            }

            await pauseConfirmationTask;
            paused = true;
        }
        finally
        {
            stateAsyncLock.Release();
        }
    }

    private async Task WaitForResumeRequestAsync(CancellationToken token)
    {
        var pendingResumeRequest = resumeRequestTcs
            ?? throw new InvalidOperationException("A resume request cannot be awaited before pause is requested.");

        await using (token.Register(() => pendingResumeRequest.TrySetCanceled(token), useSynchronizationContext: false))
        {
            await pendingResumeRequest.Task;
        }
    }

    private async Task WaitForPauseConfirmationAsync(CancellationToken token)
    {
        var pendingPauseConfirmation = pauseConfirmationTcs
            ?? throw new InvalidOperationException("Pause confirmation cannot be awaited before pause is requested.");

        await using (token.Register(() => pendingPauseConfirmation.TrySetCanceled(token), useSynchronizationContext: false))
        {
            await pendingPauseConfirmation.Task;
        }
    }

    /// <summary>
    /// Waits until pause is requested, then blocks until resumed.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    public async Task PauseIfRequestedAsync(CancellationToken token = default)
    {
        Task? resumeRequestTask = null;

        await pauseRequestAsyncLock.WaitAsync(token);

        try
        {
            if (!pauseRequested)
            {
                return;
            }

            // Confirm that the producer observed the pause request, then wait for resume.
            pauseConfirmationTcs?.TrySetResult(true);
            resumeRequestTask = WaitForResumeRequestAsync(token);
        }
        finally
        {
            pauseRequestAsyncLock.Release();
        }

        await resumeRequestTask;
    }
}

// PauseToken - consumer side
/// <summary>
/// Represents the consumer side of a <see cref="PauseTokenSource"/>.
/// </summary>
public readonly struct PauseToken
{
    private readonly PauseTokenSource source;

    /// <summary>
    /// Creates a consumer-side pause token.
    /// </summary>
    /// <param name="source">The underlying source.</param>
    public PauseToken(PauseTokenSource source)
    {
        this.source = source;
    }

    /// <summary>
    /// Returns whether the source is currently paused.
    /// </summary>
    public Task<bool> IsPaused() => source.IsPausedAsync();

    /// <summary>
    /// Waits if pause has been requested.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    public Task PauseIfRequestedAsync(CancellationToken token = default)
        => source.PauseIfRequestedAsync(token);
}
