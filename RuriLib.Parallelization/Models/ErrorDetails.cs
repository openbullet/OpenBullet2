using System;

namespace RuriLib.Parallelization.Models
{
    /// <summary>
    /// Details of an error that happened while processing a work item.
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    public class ErrorDetails<TInput>
    {
        /// <summary>The item that was being processed by the operation.</summary>
        public TInput Item { get; set; }

        /// <summary>The exception thrown by the operation.</summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Creates error details for a given <paramref name="item"/> for which the work
        /// function generated a given <paramref name="exception"/>
        /// </summary>
        public ErrorDetails(TInput item, Exception exception)
        {
            Item = item;
            Exception = exception;
        }
    }
}
