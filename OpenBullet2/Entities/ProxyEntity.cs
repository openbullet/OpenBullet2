using RuriLib.Models.Proxies;
using System;
using System.Text;

namespace OpenBullet2.Entities
{
    public class ProxyEntity : Entity
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public ProxyType Type { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public ProxyWorkingStatus Status { get; set; }
        public int Ping { get; set; }
        public DateTime LastChecked { get; set; }

        public ProxyGroupEntity Group { get; set; }

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
