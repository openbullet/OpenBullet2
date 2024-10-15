using System;

namespace RuriLib.Exceptions;

/// <summary>
/// An exception that is thrown when an error occurs while parsing LoliCode.
/// </summary>
public class LoliCodeParsingException : Exception
{
    /// <summary>
    /// The line number where the error occurred.
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Creates a new <see cref="LoliCodeParsingException"/>.
    /// </summary>
    /// <param name="lineNumber">
    /// The line number where the error occurred.
    /// </param>
    public LoliCodeParsingException(int lineNumber)
    {
        LineNumber = lineNumber;
    }

    /// <summary>
    /// Creates a new <see cref="LoliCodeParsingException"/>.
    /// </summary>
    /// <param name="lineNumber">
    /// The line number where the error occurred.
    /// </param>
    /// <param name="message">
    /// The error message.
    /// </param>
    public LoliCodeParsingException(int lineNumber, string message)
        : base($"[Line {lineNumber}] {message}")
    {
        LineNumber = lineNumber;
    }

    /// <summary>
    /// Creates a new <see cref="LoliCodeParsingException"/>.
    /// </summary>
    /// <param name="lineNumber">
    /// The line number where the error occurred.
    /// </param>
    /// <param name="message">
    /// The error message.
    /// </param>
    /// <param name="inner">
    /// The inner exception.
    /// </param>
    public LoliCodeParsingException(int lineNumber, string message, Exception inner)
        : base($"[Line {lineNumber}] {message}", inner)
    {
        LineNumber = lineNumber;
    }
}

