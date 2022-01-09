namespace RuriLib.Parallelization.Models
{
    /// <summary>
    /// Details the result of the execution of the work functions on a given input.
    /// </summary>
    /// <typeparam name="TInput">The type of input</typeparam>
    /// <typeparam name="TOutput">The type of output</typeparam>
    public class ResultDetails<TInput, TOutput>
    {
        /// <summary>The item that was being processed by the operation.</summary>
        public TInput Item { get; set; }

        /// <summary>The result returned by the operation.</summary>
        public TOutput Result { get; set; }

        /// <summary>
        /// Creates result details for a given <paramref name="item"/> for which
        /// the work function generated a given <paramref name="result"/>.
        /// </summary>
        public ResultDetails(TInput item, TOutput result)
        {
            Item = item;
            Result = result;
        }
    }
}
