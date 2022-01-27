using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization
{
    /// <summary>
    /// Parallelizer that uses the Parallel.ForEachAsync function.
    /// </summary>
    public class ParallelBasedParallelizer<TInput, TOutput> : Parallelizer<TInput, TOutput>
    {
        #region Constructors
        /// <inheritdoc/>
        public ParallelBasedParallelizer(IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
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

            throw new NotSupportedException("This parallelizer does not support pausing");
        }

        /// <inheritdoc/>
        public async override Task Resume()
        {
            await base.Resume().ConfigureAwait(false);

            throw new NotSupportedException("This parallelizer does not support resuming");
        }

        /// <inheritdoc/>
        public async override Task Stop()
        {
            await base.Stop().ConfigureAwait(false);

            throw new NotSupportedException("This parallelizer does not support soft stopping");
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

            throw new NotSupportedException("You cannot change the DoP while this parallelizer is running");
        }
        #endregion

        #region Private Methods
        // Run is executed in fire and forget mode (not awaited)
        private async void Run()
        {
            // Skip the items
            var items = workItems.Skip(skip);

            try
            {
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = degreeOfParallelism,
                    TaskScheduler = TaskScheduler.Default,
                    CancellationToken = hardCTS.Token
                };
                await Parallel.ForEachAsync(items, options, async (item, token) =>
                {
                    await taskFunction(item).ConfigureAwait(false);
                });
            }
            catch (TaskCanceledException)
            {
                // Operation aborted, don't throw the error
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
                stopwatch?.Stop();
            }
        }
        #endregion
    }
}