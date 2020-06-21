using Microsoft.AspNetCore.Components.Authorization;
using OpenBullet2.Services;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenBullet2.Auth
{
    public class JwtAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly PersistentSettingsService settings;

        public JwtAuthenticationStateProvider(PersistentSettingsService settings)
        {
            this.settings = settings;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!settings.SecurityOptions.RequireAdminLogin)
            {
                var anonymous = new ClaimsIdentity(new List<Claim>
                {
                    new Claim(ClaimTypes.Name, settings.SecurityOptions.AdminUsername),
                    new Claim(ClaimTypes.Role, "Admin")
                }, "None");
                return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
            }

            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }
}
