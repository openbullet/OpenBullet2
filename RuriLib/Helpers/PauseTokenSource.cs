using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Helpers
{
    // PauseTokenSource. Code from https://stackoverflow.com/questions/19613444/a-pattern-to-pause-resume-an-async-task
    public class PauseTokenSource
    {
        private bool paused = false;
        private bool pauseRequested = false;

        private TaskCompletionSource<bool> resumeRequestTcs;
        private TaskCompletionSource<bool> pauseConfirmationTcs;

        private readonly SemaphoreSlim stateAsyncLock = new(1);
        private readonly SemaphoreSlim pauseRequestAsyncLock = new(1);

        public PauseToken Token => new(this);

        public async Task<bool> IsPaused(CancellationToken token = default)
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
                    var resumeRequestTcs = this.resumeRequestTcs;
                    paused = false;
                    pauseRequested = false;
                    this.resumeRequestTcs = null;
                    pauseConfirmationTcs = null;
                    resumeRequestTcs.TrySetResult(true);
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

        public async Task PauseAsync(CancellationToken token = default)
        {
            await stateAsyncLock.WaitAsync(token);

            try
            {
                if (paused)
                {
                    return;
                }

                Task pauseConfirmationTask = null;
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
            await using (token.Register(() => resumeRequestTcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                await resumeRequestTcs.Task;
            }
        }

        private async Task WaitForPauseConfirmationAsync(CancellationToken token)
        {
            await using (token.Register(() => pauseConfirmationTcs.TrySetCanceled(), useSynchronizationContext: false))
            {
                await pauseConfirmationTcs.Task;
            }
        }

        public async Task PauseIfRequestedAsync(CancellationToken token = default)
        {
            Task resumeRequestTask = null;

            await pauseRequestAsyncLock.WaitAsync(token);

            try
            {
                if (!pauseRequested)
                {
                    return;
                }

                resumeRequestTask = WaitForResumeRequestAsync(token);
                pauseConfirmationTcs.TrySetResult(true);
            }
            finally
            {
                pauseRequestAsyncLock.Release();
            }

            await resumeRequestTask;
        }
    }

    // PauseToken - consumer side
    public struct PauseToken
    {
        private readonly PauseTokenSource source;

        public PauseToken(PauseTokenSource source)
        {
            this.source = source;
        }

        public Task<bool> IsPaused() => source.IsPaused();

        public Task PauseIfRequestedAsync(CancellationToken token = default)
            => source.PauseIfRequestedAsync(token);
    }
}
