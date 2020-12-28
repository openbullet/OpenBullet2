using RuriLib.Models.Proxies;
using System;

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
    }
}
