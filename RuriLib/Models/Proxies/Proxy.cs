using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Models.Proxies
{
    public class Proxy
    {
        public int Id { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public ProxyType Type { get; set; }

        public ProxyWorkingStatus WorkingStatus { get; set; } = ProxyWorkingStatus.Untested;
        public string Country { get; set; } = "Unknown";
        public int Ping { get; set; } = 0;

        public DateTime LastUsed { get; set; }
        public DateTime LastChecked { get; set; }
        public DateTime LastBanned { get; set; }

        public int TotalUses { get; set; } = 0;
        public int BeingUsedBy { get; set; } = 0;
        public ProxyStatus ProxyStatus { get; set; } = ProxyStatus.Available;

        public bool NeedsAuthentication => !string.IsNullOrWhiteSpace(Username);

        public Proxy(string host, int port, ProxyType type = ProxyType.Http, string username = "", string password = "")
        {
            Host = host;
            Port = port;
            Type = type;
            Username = username;
            Password = password;
        }

        /// <summary>
        /// Parses a Proxy from a string. See examples for accepted inputs.
        /// </summary>
        /// <example>Proxy.Parse("127.0.0.1:8000")</example>
        /// <example>Proxy.Parse("127.0.0.1:8000:username:password")</example>
        /// <example>Proxy.Parse("(socks5)127.0.0.1:8000")</example>
        public static bool TryParse(string proxyString, out Proxy proxy, ProxyType defaultType = ProxyType.Http,
            string defaultUsername = "", string defaultPassword = "")
        {
            try
            {
                proxy = Parse(proxyString, defaultType, defaultUsername, defaultPassword);
                return true;
            }
            catch
            {
                proxy = null;
                return false;
            }
        }

        /// <summary>
        /// Parses a Proxy from a string. See examples for accepted inputs.
        /// </summary>
        /// <example>Proxy.Parse("127.0.0.1:8000")</example>
        /// <example>Proxy.Parse("127.0.0.1:8000:username:password")</example>
        /// <example>Proxy.Parse("(socks5)127.0.0.1:8000")</example>
        public static Proxy Parse(string proxyString, ProxyType defaultType = ProxyType.Http,
            string defaultUsername = "", string defaultPassword = "")
        {
            if (proxyString == null)
                throw new ArgumentNullException(nameof(proxyString));

            if (defaultUsername == null)
                throw new ArgumentNullException(nameof(defaultUsername));

            if (defaultPassword == null)
                throw new ArgumentNullException(nameof(defaultPassword));

            Proxy proxy = new Proxy(string.Empty, 0, defaultType, defaultUsername, defaultPassword);

            // If the type was specified, parse it and remove it from the string
            if (proxyString.StartsWith('('))
            {
                var groups = Regex.Match(proxyString, @"^\((.*)\)").Groups;

                if (Enum.TryParse<ProxyType>(groups[1].Value, true, out var type))
                    proxy.Type = type;
                else
                    throw new FormatException("Invalid proxy type");

                proxyString = Regex.Replace(proxyString, @"^\((.*)\)", "");
            }

            if (!proxyString.Contains(':'))
                throw new FormatException("Expected at least 2 colon-separated fields");

            var fields = proxyString.Split(':');
            proxy.Host = fields[0];

            if (int.TryParse(fields[1], out int port))
                proxy.Port = port;
            else
                throw new FormatException("The proxy port must be an integer");

            if (fields.Length == 3)
                throw new FormatException("Expected 4 colon-separated fields, got 3");

            // Set the other two if they are present
            if (fields.Length > 2)
            {
                proxy.Username = fields[2];
                proxy.Password = fields[3];
            }

            return proxy;
        }

        public override int GetHashCode()
        {
            return Host.GetHashCode() + Port.GetHashCode() + Type.GetHashCode()
                + Username.GetHashCode() + Password.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (Type != ProxyType.Http)
                sb.Append($"({Type})");

            sb.Append($"{Host}:{Port}");

            if (!string.IsNullOrWhiteSpace(Username))
                sb.Append($":{Username}:{Password}");

            return sb.ToString();
        }
    }
}
