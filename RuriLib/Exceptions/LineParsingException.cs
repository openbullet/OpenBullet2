using System;

namespace RuriLib.Exceptions;

/// <summary>
/// An exception that is thrown when a single LoliCode line cannot be parsed.
/// </summary>
public class LineParsingException : FormatException
{
    /// <summary>
    /// The column number where the error occurred, if known.
    /// </summary>
    public int? ColumnNumber { get; }

    /// <summary>
    /// Creates a new <see cref="LineParsingException"/>.
    /// </summary>
    /// <param name="message">
    /// The error message.
    /// </param>
    public LineParsingException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new <see cref="LineParsingException"/>.
    /// </summary>
    /// <param name="columnNumber">
    /// The column number where the error occurred.
    /// </param>
    /// <param name="message">
    /// The error message.
    /// </param>
    public LineParsingException(int columnNumber, string message)
        : base(message)
    {
        ColumnNumber = columnNumber;
    }

    /// <summary>
    /// Creates a new <see cref="LineParsingException"/>.
    /// </summary>
    /// <param name="columnNumber">
    /// The column number where the error occurred.
    /// </param>
    /// <param name="message">
    /// The error message.
    /// </param>
    /// <param name="inner">
    /// The inner exception.
    /// </param>
    public LineParsingException(int columnNumber, string message, Exception inner)
        : base(message, inner)
    {
        ColumnNumber = columnNumber;
    }
}
