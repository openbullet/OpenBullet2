using System;

namespace RuriLib.Http.Exceptions;

/// <summary>
/// An exception that is thrown when an HTTP request fails.
/// </summary>
public class RLHttpException : Exception
{
    /// <summary>
    /// Creates an RLHttpException with a <paramref name="message"/>.
    /// </summary>
    public RLHttpException(string message) : base(message) { }
}
