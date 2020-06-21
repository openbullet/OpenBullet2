namespace OpenBullet2.Models
{
    public class SecurityOptions
    {
        public bool AllowSystemWideFileAccess { get; private set; }
        public bool RequireAdminLogin { get; private set; }
        public string AdminUsername { get; private set; }
        public string AdminPasswordHash { get; private set; }
        public string JwtKey { get; private set; }
    }
}
