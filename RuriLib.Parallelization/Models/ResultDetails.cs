namespace RuriLib.Parallelization.Models
{
    public class ResultDetails<TInput, TOutput>
    {
        /// <summary>The item that was being processed by the operation.</summary>
        public TInput Item { get; set; }

        /// <summary>The result returned by the operation.</summary>
        public TOutput Result { get; set; }

        public ResultDetails(TInput item, TOutput result)
        {
            Item = item;
            Result = result;
        }
    }
}
