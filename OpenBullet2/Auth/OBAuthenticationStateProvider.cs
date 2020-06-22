using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.IdentityModel.Tokens;
using OpenBullet2.Exceptions;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenBullet2.Auth
{
    public class OBAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService localStorage;
        private readonly PersistentSettingsService settings;
        private readonly IGuestRepository guestRepo;
        private readonly JwtSecurityTokenHandler handler;
        private readonly TokenValidationParameters validationParams;

        public OBAuthenticationStateProvider(ILocalStorageService localStorage, PersistentSettingsService settings,
            IGuestRepository guestRepo)
        {
            this.localStorage = localStorage;
            this.settings = settings;
            this.guestRepo = guestRepo;
            handler = new JwtSecurityTokenHandler();
            validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(settings.OpenBulletSettings.SecuritySettings.JwtKey),
                ClockSkew = TimeSpan.Zero
            };
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // If we didn't enable admin login, always return an authenticated admin user
            if (!settings.OpenBulletSettings.SecuritySettings.RequireAdminLogin)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, settings.OpenBulletSettings.SecuritySettings.AdminUsername),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, "noAuth");
                var admin = new ClaimsPrincipal(identity);
                return new AuthenticationState(admin);
            }

            var jwt = await localStorage.GetItemAsync<string>("jwt");

            try 
            {
                var authenticatedUser = handler.ValidateToken(jwt, validationParams, out SecurityToken validatedToken);
                return new AuthenticationState(authenticatedUser);
            }
            catch
            {
                // Token not present, expired or forged
                var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
                return new AuthenticationState(anonymousUser);
            }
        }

        public async Task AuthenticateUser(string username, string password)
        {
            if (settings.OpenBulletSettings.SecuritySettings.AdminUsername == username)
                await AuthenticateAdmin(username, password);
            else
                await AuthenticateGuest(username, password);
        }

        private async Task AuthenticateAdmin(string username, string password)
        {
            if (!BCrypt.Net.BCrypt.Verify(password, settings.OpenBulletSettings.SecuritySettings.AdminPasswordHash))
                throw new UnauthorizedAccessException("Invalid password");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var jwt = BuildAdminToken(claims);
            await localStorage.SetItemAsync("jwt", jwt);
            
            var identity = new ClaimsIdentity(claims, "adminAuth");
            var admin = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(admin)));
        }

        private async Task AuthenticateGuest(string username, string password)
        {
            var entity = guestRepo.GetAll().FirstOrDefault(g => g.Username == username);

            if (entity == null)
                throw new EntryNotFoundException("Could not find a guest with the given username");

            if (!BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash))
                throw new UnauthorizedAccessException("Invalid password");

            if (DateTime.UtcNow > entity.AccessExpiration)
                throw new UnauthorizedAccessException("Access to this guest account has expired");

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Guest")
            };

            var jwt = BuildGuestToken(claims);
            await localStorage.SetItemAsync("jwt", jwt);

            var identity = new ClaimsIdentity(claims, "guestAuth");
            var guest = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(guest)));
        }

        private string BuildAdminToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(settings.OpenBulletSettings.SecuritySettings.JwtKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(settings.OpenBulletSettings.SecuritySettings.AdminSessionLifetimeHours);

            JwtSecurityToken token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
            return handler.WriteToken(token);
        }

        private string BuildGuestToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(settings.OpenBulletSettings.SecuritySettings.JwtKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(settings.OpenBulletSettings.SecuritySettings.GuestSessionLifetimeHours);

            JwtSecurityToken token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
            return handler.WriteToken(token);
        }
    }
}
