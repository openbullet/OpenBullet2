using System;

namespace RuriLib.Exceptions;

/// <summary>
/// An exception that is thrown when a Proxy could not be parsed.
/// </summary>
public class InvalidProxyException : Exception
{
    /// <summary>
    /// Creates a <see cref="InvalidProxyException"/> with a message that contains the invalid proxy.
    /// </summary>
    /// <param name="proxy">
    /// The invalid proxy that could not be parsed.
    /// </param>
    public InvalidProxyException(string proxy)
        : base($"The proxy {proxy} could not be parsed")
    {

    }
}
