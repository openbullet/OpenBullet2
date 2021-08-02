using System.Security.Cryptography;

namespace OpenBullet2.Core.Models.Settings
{
    /// <summary>
    /// Settings related to security.
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// Whether to allow OpenBullet2 (mainly blocks and file system viewer) to access
        /// the whole system or only the UserData folder and its subfolders.
        /// </summary>
        public bool AllowSystemWideFileAccess { get; set; } = false;

        /// <summary>
        /// Whether to require admin login when accessing the UI. Use this when exposing
        /// an OpenBullet 2 instance on the unprotected internet.
        /// </summary>
        public bool RequireAdminLogin { get; set; } = false;

        /// <summary>
        /// The username for the admin user.
        /// </summary>
        public string AdminUsername { get; set; } = "admin";

        /// <summary>
        /// The bcrypt hash of the admin user's password.
        /// </summary>
        public string AdminPasswordHash { get; set; }

        /// <summary>
        /// The JWT key that this application will use when issuing authentication tokens.
        /// For security reasons this should be randomly generated via the <see cref="GenerateJwtKey"/> method.
        /// </summary>
        public byte[] JwtKey { get; set; }

        /// <summary>
        /// The number of hours that the admin session will last before requiring another login.
        /// </summary>
        public int AdminSessionLifetimeHours { get; set; } = 24;

        /// <summary>
        /// The number of hours that the guest session will last before requiring another login.
        /// </summary>
        public int GuestSessionLifetimeHours { get; set; } = 24;

        /// <summary>
        /// Whether to use HTTPS redirection when the application is accessed via HTTP.
        /// </summary>
        public bool HttpsRedirect { get; set; } = false;

        /// <summary>
        /// Generates a random JWT key to use in order to sign JWT tokens issued by the application
        /// to both the admin and guests.
        /// </summary>
        public SecuritySettings GenerateJwtKey()
        {
            JwtKey = new byte[64];
            var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(JwtKey);
            return this;
        }

        /// <summary>
        /// Sets a new admin password.
        /// </summary>
        public SecuritySettings SetupAdminPassword(string password)
        {
            AdminPasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            return this;
        }
    }
}
