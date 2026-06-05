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
    /// The column number where the error occurred, if known.
    /// </summary>
    public int? ColumnNumber { get; set; }

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
    /// <param name="columnNumber">
    /// The column number where the error occurred.
    /// </param>
    /// <param name="message">
    /// The error message.
    /// </param>
    public LoliCodeParsingException(int lineNumber, int columnNumber, string message)
        : base(FormatMessage(lineNumber, columnNumber, message))
    {
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
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

    /// <summary>
    /// Creates a new <see cref="LoliCodeParsingException"/>.
    /// </summary>
    /// <param name="lineNumber">
    /// The line number where the error occurred.
    /// </param>
    /// <param name="columnNumber">
    /// The column number where the error occurred.
    /// </param>
    /// <param name="message">
    /// The error message.
    /// </param>
    /// <param name="inner">
    /// The inner exception.
    /// </param>
    public LoliCodeParsingException(int lineNumber, int columnNumber, string message, Exception inner)
        : base(FormatMessage(lineNumber, columnNumber, message), inner)
    {
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
    }

    private static string FormatMessage(int lineNumber, int columnNumber, string message)
        => $"[Line {lineNumber}, Column {columnNumber}] {message}";
}

