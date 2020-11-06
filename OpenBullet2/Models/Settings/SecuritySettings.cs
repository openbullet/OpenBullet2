using System.Security.Cryptography;

namespace OpenBullet2.Models.Settings
{
    public class SecuritySettings
    {
        public bool AllowSystemWideFileAccess { get; set; } = false;
        public bool RequireAdminLogin { get; set; } = false;
        public string AdminUsername { get; set; } = "admin";
        public string AdminPasswordHash { get; set; }
        public byte[] JwtKey { get; set; }
        public int AdminSessionLifetimeHours { get; set; } = 24;
        public int GuestSessionLifetimeHours { get; set; } = 24;
        public bool HttpsRedirect { get; set; } = false;

        public SecuritySettings GenerateJwtKey()
        {
            JwtKey = new byte[64];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(JwtKey);
            return this;
        }

        public SecuritySettings SetupAdminPassword(string password)
        {
            AdminPasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            return this;
        }
    }
}
