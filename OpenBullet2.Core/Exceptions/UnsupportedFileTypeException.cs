using System;

namespace OpenBullet2.Core.Exceptions;

/// <summary>
/// The exception that is thrown when a file type is not supported.
/// </summary>
public class UnsupportedFileTypeException : Exception
{
    /// <summary>
    /// Creates a new <see cref="UnsupportedFileTypeException"/> with a message.
    /// </summary>
    public UnsupportedFileTypeException(string message) : base(message) { }
}
