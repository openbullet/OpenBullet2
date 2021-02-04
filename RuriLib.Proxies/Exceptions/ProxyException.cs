using System;

namespace RuriLib.Proxies.Exceptions
{
    /// <summary>
    /// Represents errors that occur during proxy execution.
    /// </summary>
    public class ProxyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyException"/> with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ProxyException(string message)
            : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyException"/> with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a <see langword="null"/> reference.</param>
        public ProxyException(string message, Exception innerException)
            : base(message, innerException) { }

    }
}
