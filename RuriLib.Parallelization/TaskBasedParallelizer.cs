using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization
{
    /// <summary>
    /// Parallelizer that expoits batches of multiple tasks and the WaitAll function.
    /// </summary>
    public class TaskBasedParallelizer<TInput, TOutput> : Parallelizer<TInput, TOutput>
    {
        #region Private Fields
        private int BatchSize => MaxDegreeOfParallelism * 2;
        private SemaphoreSlim semaphore;
        private ConcurrentQueue<TInput> queue;
        private int savedDOP;
        private bool dopDecreaseRequested;
        #endregion

        #region Constructors
        /// <inheritdoc/>
        public TaskBasedParallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
            int degreeOfParallelism, long totalAmount, int skip = 0, int maxDegreeOfParallelism = 200)
            : base(workItems, workFunction, degreeOfParallelism, totalAmount, skip, maxDegreeOfParallelism)
        {

        }
        #endregion

        #region Public Methods
        /// <inheritdoc/>
        public async override Task Start()
        {
            await base.Start().ConfigureAwait(false);

            stopwatch.Restart();
            Status = ParallelizerStatus.Running;
            _ = Task.Run(() => Run()).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async override Task Pause()
        {
            await base.Pause().ConfigureAwait(false);

            Status = ParallelizerStatus.Pausing;
            savedDOP = degreeOfParallelism;
            await ChangeDegreeOfParallelism(0).ConfigureAwait(false);
            Status = ParallelizerStatus.Paused;
            stopwatch.Stop();
        }

        /// <inheritdoc/>
        public async override Task Resume()
        {
            await base.Resume().ConfigureAwait(false);

            Status = ParallelizerStatus.Resuming;
            await ChangeDegreeOfParallelism(savedDOP).ConfigureAwait(false);
            Status = ParallelizerStatus.Running;
            stopwatch.Start();
        }

        /// <inheritdoc/>
        public async override Task Stop()
        {
            await base.Stop().ConfigureAwait(false);

            Status = ParallelizerStatus.Stopping;
            softCTS.Cancel();
            await WaitCompletion().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async override Task Abort()
        {
            await base.Abort().ConfigureAwait(false);

            Status = ParallelizerStatus.Stopping;
            hardCTS.Cancel();
            softCTS.Cancel();
            await WaitCompletion().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async override Task ChangeDegreeOfParallelism(int newValue)
        {
            await base.ChangeDegreeOfParallelism(newValue);

            if (Status == ParallelizerStatus.Idle)
            {
                degreeOfParallelism = newValue;
                return;
            }
            else if (Status == ParallelizerStatus.Paused)
            {
                savedDOP = newValue;
                return;
            }

            if (newValue == degreeOfParallelism)
            {
                return;
            }
            else if (newValue > degreeOfParallelism)
            {
                semaphore.Release(newValue - degreeOfParallelism);
            }
            else
            {
                dopDecreaseRequested = true;
                for (var i = 0; i < degreeOfParallelism - newValue; ++i)
                {
                    await semaphore.WaitAsync().ConfigureAwait(false);
                }
                dopDecreaseRequested = false;
            }

            degreeOfParallelism = newValue;
        }
        #endregion

        #region Private Methods
        // Run is executed in fire and forget mode (not awaited)
        private async void Run()
        {
            semaphore = new SemaphoreSlim(degreeOfParallelism, MaxDegreeOfParallelism);
            dopDecreaseRequested = false;

            // Skip the items
            using var items = workItems.Skip(skip).GetEnumerator();

            // Create the queue
            queue = new ConcurrentQueue<TInput>();

            // Enqueue the first batch (at most BatchSize items)
            while (queue.Count < BatchSize && items.MoveNext())
            {
                queue.Enqueue(items.Current);
            }

            try
            {
                // While there are items in the queue and we didn't cancel, dequeue one, wait and then
                // queue another task if there are more to queue
                while (!queue.IsEmpty && !softCTS.IsCancellationRequested)
                {
                    WAIT:

                    // Wait for the semaphore
                    await semaphore.WaitAsync(softCTS.Token).ConfigureAwait(false);

                    if (softCTS.IsCancellationRequested)
                        break;

                    if (dopDecreaseRequested || IsCPMLimited())
                    {
                        UpdateCPM();
                        semaphore?.Release();
                        goto WAIT;
                    }

                    // If the current batch is running out
                    if (queue.Count < MaxDegreeOfParallelism)
                    {
                        // Queue more items until the BatchSize is reached OR until the enumeration finished
                        while (queue.Count < BatchSize && items.MoveNext())
                        {
                            queue.Enqueue(items.Current);
                        }
                    }

                    // If we can dequeue an item, run it
                    if (queue.TryDequeue(out TInput item))
                    {
                        // The task will release its slot no matter what
                        _ = taskFunction.Invoke(item)
                            .ContinueWith(_ => semaphore?.Release())
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        semaphore?.Release();
                    }
                }

                // Wait for every remaining task from the last batch to finish unless aborted
                while (Progress < 1 && !hardCTS.IsCancellationRequested)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Wait for current tasks to finish unless aborted
                while (semaphore.CurrentCount < degreeOfParallelism && !hardCTS.IsCancellationRequested)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                OnCompleted();
                Status = ParallelizerStatus.Idle;
                hardCTS?.Dispose();
                softCTS?.Dispose();
                semaphore?.Dispose();
                semaphore = null;
                stopwatch?.Stop();
            }
        }
        #endregion
    }
}