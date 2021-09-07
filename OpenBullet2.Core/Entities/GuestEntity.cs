using System;

namespace OpenBullet2.Core.Entities
{
    /// <summary>
    /// This entity stores a guest user of OpenBullet 2.
    /// </summary>
    public class GuestEntity : Entity
    {
        /// <summary>
        /// The username that the guest uses to log in.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The bcrypt hash of the password of the guest.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// The time when access will expire for this guest.
        /// </summary>
        public DateTime AccessExpiration { get; set; }

        /// <summary>
        /// A comma-separated list of IPv4 or IPv6 addresses that the guest
        /// is allowed to use when connecting to the remote instance of OpenBullet 2.
        /// These can include masked IP ranges and static DNS.
        /// </summary>
        public string AllowedAddresses { get; set; }
    }
}
