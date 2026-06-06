using System;

namespace RuriLib.Proxies.Exceptions;

/// <summary>
/// Represents errors that indicate the proxy itself is unusable.
/// </summary>
public class BadProxyException : ProxyException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BadProxyException"/> class.
    /// </summary>
    public BadProxyException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadProxyException"/> class.
    /// </summary>
    public BadProxyException(string message, Exception innerException)
        : base(message, innerException) { }
}
