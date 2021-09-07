using RuriLib.Models.Proxies;
using System;
using System.Text;

namespace OpenBullet2.Core.Entities
{
    /// <summary>
    /// This entity stores a proxy that belongs to a proxy group.
    /// </summary>
    public class ProxyEntity : Entity
    {
        /// <summary>
        /// The host on which the proxy server is running.
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The port on which the proxy server is listening.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The protocol used by the proxy server to open a proxy tunnel.
        /// </summary>
        public ProxyType Type { get; set; }

        /// <summary>
        /// The username, if required by the proxy server.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password, if required by the proxy server.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The country of the proxy, detected after checking it with a geolocalization service.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// The working status of the proxy.
        /// </summary>
        public ProxyWorkingStatus Status { get; set; }

        /// <summary>
        /// The ping of the proxy in milliseconds.
        /// </summary>
        public int Ping { get; set; }

        /// <summary>
        /// The last time the proxy was checked.
        /// </summary>
        public DateTime LastChecked { get; set; }

        /// <summary>
        /// The proxy group to which the proxy belongs to.
        /// </summary>
        public ProxyGroupEntity Group { get; set; }

        /// <summary>
        /// Returns a string representation of the proxy.
        /// For example <code>(Socks5)192.168.1.1:8080:username:password</code>
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Type != ProxyType.Http)
            {
                sb.Append($"({Type})");
            }

            sb.Append($"{Host}:{Port}");

            if (!string.IsNullOrWhiteSpace(Username))
            {
                sb.Append($":{Username}:{Password}");
            }

            return sb.ToString();
        }
    }
}
