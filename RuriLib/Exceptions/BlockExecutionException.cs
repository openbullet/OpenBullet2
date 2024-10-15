using System;

namespace RuriLib.Exceptions;

/// <summary>
/// The exception that is thrown when a block encounters an error during execution.
/// </summary>
public class BlockExecutionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockExecutionException"/> class.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error.
    /// </param>
    public BlockExecutionException(string message)
        : base(message)
    {

    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BlockExecutionException"/> class.
    /// </summary>
    /// <param name="message">
    /// The message that describes the error.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public BlockExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
