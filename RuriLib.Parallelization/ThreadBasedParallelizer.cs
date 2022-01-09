using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization
{
    /// <summary>
    /// Parallelizer that expoits a custom pool of threads.
    /// </summary>
    public class ThreadBasedParallelizer<TInput, TOutput> : Parallelizer<TInput, TOutput>
    {
        #region Private Fields
        private readonly List<Thread> threadPool = new();
        #endregion

        #region Constructors
        /// <inheritdoc/>
        public ThreadBasedParallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
            int degreeOfParallelism, long totalAmount, int skip = 0, int maxDegreeOfParallelism = 200)
            : base(workItems, workFunction, degreeOfParallelism, totalAmount, skip, maxDegreeOfParallelism)
        {

        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public async override Task Start()
        {
            await base.Start();

            stopwatch.Restart();
            Status = ParallelizerStatus.Running;
            _ = Task.Run(() => Run()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async override Task Pause()
        {
            await base.Pause();

            Status = ParallelizerStatus.Pausing;
            await WaitCurrentWorkCompletion();
            Status = ParallelizerStatus.Paused;
            stopwatch.Stop();
        }

        /// <inheritdoc/>
        public async override Task Resume()
        {
            await base.Resume();

            Status = ParallelizerStatus.Running;
            stopwatch.Start();
        }

        /// <inheritdoc/>
        public async override Task Stop()
        {
            await base.Stop();

            Status = ParallelizerStatus.Stopping;
            softCTS.Cancel();
            await WaitCompletion().ConfigureAwait(false);
            stopwatch.Stop();
        }

        /// <inheritdoc/>
        public async override Task Abort()
        {
            await base.Abort();

            Status = ParallelizerStatus.Stopping;
            hardCTS.Cancel();
            softCTS.Cancel();
            await WaitCompletion().ConfigureAwait(false);
            stopwatch.Stop();
        }

        /// <inheritdoc/>
        public async override Task ChangeDegreeOfParallelism(int newValue)
        {
            await base.ChangeDegreeOfParallelism(newValue);

            degreeOfParallelism = newValue;
        }
        #endregion

        #region Private Methods
        // Run is executed in fire and forget mode (not awaited)
        private async void Run()
        {
            // Skip the items
            using var items = workItems.Skip(skip).GetEnumerator();

            while (items.MoveNext())
            {
                WAIT:

                // If we paused, stay idle
                if (Status == ParallelizerStatus.Pausing || Status == ParallelizerStatus.Paused)
                {
                    await Task.Delay(1000);
                    goto WAIT;
                }

                // If we canceled the loop
                if (softCTS.IsCancellationRequested)
                {
                    break;
                }

                // If we haven't filled the thread pool yet, start a new thread
                // (e.g. if we're at the beginning or the increased the DOP)
                if (threadPool.Count < degreeOfParallelism)
                {
                    StartNewThread(items.Current);
                }
                // Otherwise if we already filled the thread pool
                else
                {
                    // If we exceeded the CPM threshold, update CPM and go back to waiting
                    if (IsCPMLimited())
                    {
                        UpdateCPM();
                        await Task.Delay(100);
                        goto WAIT;
                    }

                    // Search for the first idle thread
                    var firstFree = threadPool.FirstOrDefault(t => !t.IsAlive);

                    // If there is none, go back to waiting
                    if (firstFree == null)
                    {
                        await Task.Delay(100);
                        goto WAIT;
                    }

                    // Otherwise remove it
                    threadPool.Remove(firstFree);

                    // If there's space for a new thread, start it
                    if (threadPool.Count < degreeOfParallelism)
                    {
                        StartNewThread(items.Current);
                    }
                    // Otherwise go back to waiting
                    else
                    {
                        await Task.Delay(100);
                        goto WAIT;
                    }
                }
            }

            // Wait until ongoing threads finish
            await WaitCurrentWorkCompletion();

            OnCompleted();
            Status = ParallelizerStatus.Idle;
            hardCTS.Dispose();
            softCTS.Dispose();
            stopwatch.Stop();
        }

        // Creates and starts a thread, given a work item
        private void StartNewThread(TInput item)
        {
            var thread = new Thread(new ParameterizedThreadStart(ThreadWork));
            threadPool.Add(thread);
            thread.Start(item);
        }

        // Sync method to be passed to a thread
        private void ThreadWork(object input)
            => taskFunction((TInput)input).Wait();

        // Wait until the current round is over (if we didn't cancel, it's the last one)
        private async Task WaitCurrentWorkCompletion()
        {
            while (threadPool.Any(t => t.IsAlive))
            {
                await Task.Delay(100);
            }
        }
        #endregion
    }
}
