using OpenBullet2.Models;
using Microsoft.Extensions.Configuration;

namespace OpenBullet2.Services
{
    public class PersistentSettingsService
    {
        public SecurityOptions SecurityOptions { get; set; } = new SecurityOptions();

        public PersistentSettingsService(IConfiguration configuration)
        {
            SecurityOptions = configuration.GetSection("Security").Get<SecurityOptions>();
        }
    }
}
