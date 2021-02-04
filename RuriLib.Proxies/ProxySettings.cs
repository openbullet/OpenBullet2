using System;
using System.Net;

namespace RuriLib.Proxies
{
    /// <summary>
    /// Settings for <see cref="ProxyClient"/>.
    /// </summary>
    public class ProxySettings
    {
        /// <summary>
        /// Gets or sets the credentials to submit to the proxy server for authentication.
        /// </summary>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// The hostname or ip of the proxy server.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The port on which the proxy server is listening.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the amount of time the <see cref="ProxyClient"/>
        /// will wait to connect to the proxy server.
        /// </summary>
        public TimeSpan ConnectTimeout { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Gets or sets the amount of time the <see cref="ProxyClient"/>
        /// will wait for read or wait data from the proxy server.
        /// </summary>
        public TimeSpan ReadWriteTimeOut { get; set; } = TimeSpan.FromSeconds(10);
    }
}
