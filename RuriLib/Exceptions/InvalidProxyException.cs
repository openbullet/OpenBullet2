using System;

namespace RuriLib.Exceptions
{
    /// <summary>
    /// An exception that is thrown when a Proxy could not be parsed.
    /// </summary>
    public class InvalidProxyException : Exception
    {
        public InvalidProxyException(string proxy)
            : base($"The proxy {proxy} could not be parsed")
        {

        }
    }
}
