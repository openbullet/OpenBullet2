using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenBullet2.Auth
{
    public class OBAuthenticationStateProvider : AuthenticationStateProvider
    {
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var anonymous = new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Ruri"),
                new Claim(ClaimTypes.Role, "Admin")
            });
            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonymous)));
        }
    }
}
