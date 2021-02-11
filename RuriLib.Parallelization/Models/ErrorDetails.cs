using System;

namespace RuriLib.Parallelization.Models
{
    public class ErrorDetails<TInput>
    {
        /// <summary>The item that was being processed by the operation.</summary>
        public TInput Item { get; set; }

        /// <summary>The exception thrown by the operation.</summary>
        public Exception Exception { get; set; }

        public ErrorDetails(TInput item, Exception ex)
        {
            Item = item;
            Exception = ex;
        }
    }
}
