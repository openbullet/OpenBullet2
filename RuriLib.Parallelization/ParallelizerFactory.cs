using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization
{
    /// <summary>
    /// Collection of factory methods to help create parallelizers.
    /// </summary>
    /// <typeparam name="TInput">The type of input that the parallelizer accepts</typeparam>
    /// <typeparam name="TOutput">The type of output that the parallelizer produces</typeparam>
    public static class ParallelizerFactory<TInput, TOutput>
    {
        /// <summary>
        /// Creates a parallelizer from the given settings.
        /// </summary>
        /// <param name="type">The type of parallelizer to use</param>
        /// <param name="workItems">The collection of items that need to be processed in parallel</param>
        /// <param name="workFunction">The work function that asynchronously processes each item and produces an output</param>
        /// <param name="degreeOfParallelism">The maximum number of items that can be processed concurrently</param>
        /// <param name="totalAmount">The total amount of items that are expected to be enumerated (for Progress purposes)</param>
        /// <param name="skip">The amount of items to skip from the beginning (to restore previously aborted sessions)</param>
        /// <param name="maxDegreeOfParallelism">The maximum value that <paramref name="degreeOfParallelism"/> can assume when it is
        /// changed with the <see cref="Parallelizer{TInput, TOutput}.ChangeDegreeOfParallelism(int)"/> method</param>
        public static Parallelizer<TInput, TOutput> Create(ParallelizerType type,
            IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
                int degreeOfParallelism, long totalAmount, int skip = 0, int maxDegreeOfParallelism = 200)
        {
            var pType = type switch
            {
                ParallelizerType.TaskBased => typeof(TaskBasedParallelizer<TInput, TOutput>),
                ParallelizerType.ThreadBased => typeof(ThreadBasedParallelizer<TInput, TOutput>),
                ParallelizerType.ParallelBased => typeof(ParallelBasedParallelizer<TInput, TOutput>),
                _ => throw new NotImplementedException()
            };

            var instance = Activator.CreateInstance(pType, workItems, workFunction, degreeOfParallelism, 
                totalAmount, skip, maxDegreeOfParallelism);

            return instance as Parallelizer<TInput, TOutput>;
        }
    }
}
