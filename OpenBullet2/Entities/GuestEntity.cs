using System;

namespace OpenBullet2.Entities
{
    public class GuestEntity : Entity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime AccessExpiration { get; set; }
        public string AllowedAddresses { get; set; }
    }
}
