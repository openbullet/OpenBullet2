using RuriLib.Threading.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Threading
{
    /// <summary>
    /// Provides a managed way to execute parallelized work.
    /// </summary>
    /// <typeparam name="TInput">The type of the workload items</typeparam>
    /// <typeparam name="TOutput">The type of the results</typeparam>
    public interface IThreadManager<TInput, TOutput>
    {
        ThreadManagerStatus Status { get; }

        /// <summary>
        /// Retrieves the current progress in the interval [0, 1].
        /// The progress is -1 if the manager hasn't been started yet.
        /// </summary>
        float Progress { get; }

        /// <summary>
        /// Retrieves the completed work per minute.
        /// </summary>
        int CPM { get; }

        DateTime StartTime { get; }
        DateTime ETA { get; }
        TimeSpan Elapsed { get; }
        TimeSpan Remaining { get; }

        event EventHandler<ErrorDetails<TInput>> OnTaskError;
        event EventHandler<Exception> OnError;
        event EventHandler<ResultDetails<TInput, TOutput>> OnResult;
        event EventHandler<float> OnProgress;
        event EventHandler OnCompleted;
        event EventHandler<ThreadManagerStatus> OnStatusChanged;

        /// <summary>
        /// Aborts the execution without waiting for the current work to finish.
        /// </summary>
        Task Abort();

        /// <summary>
        /// Pauses the execution.
        /// </summary>
        Task Pause();

        /// <summary>
        /// Resumes a paused execution.
        /// </summary>
        Task Resume();

        /// <summary>
        /// Dynamically changes the degree of parallelism.
        /// </summary>
        Task SetParallelThreads(int newValue);

        /// <summary>
        /// Starts the execution.
        /// </summary>
        Task Start();

        /// <summary>
        /// Stops the execution (waits for the current items to finish).
        /// </summary>
        /// <returns></returns>
        Task Stop();

        /// <summary>
        /// An awaitable handler that completes when the <see cref="Status"/> is <see cref="ThreadManagerStatus.Idle"/>.
        /// </summary>
        Task WaitCompletion(CancellationToken cancellationToken = default);
    }
}
