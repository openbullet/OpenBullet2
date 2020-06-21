using System;

namespace OpenBullet2.DTOs
{
    public class UserToken
    {
        public string Jwt { get; set; }
        public DateTime Expiration { get; set; }
    }
}
