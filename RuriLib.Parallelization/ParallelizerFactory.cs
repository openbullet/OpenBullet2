using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RuriLib.Parallelization
{
    public static class ParallelizerFactory<TInput, TOutput>
    {
        public static Parallelizer<TInput, TOutput> Create(ParallelizerType type,
            IEnumerable<TInput> workItems, Func<TInput, CancellationToken, Task<TOutput>> workFunction,
                int degreeOfParallelism, int totalAmount, int skip = 0)
        {
            var pType = type switch
            {
                ParallelizerType.TaskBased => typeof(TaskBasedParallelizer<TInput, TOutput>),
                ParallelizerType.ThreadBased => typeof(ThreadBasedParallelizer<TInput, TOutput>),
                _ => throw new NotImplementedException()
            };

            var instance = Activator.CreateInstance(pType, workItems, workFunction, degreeOfParallelism, totalAmount, skip);
            return instance as Parallelizer<TInput, TOutput>;
        }
    }
}
