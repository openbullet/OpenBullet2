using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenBullet2.Exceptions;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Auth
{
    /// <summary>
    /// Provides the authentication state of a session and info about the currently
    /// logged in user.
    /// </summary>
    public class OBAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService localStorage;
        private readonly OpenBulletSettingsService settingsService;
        private readonly IGuestRepository guestRepo;
        private readonly JwtValidationService jwtValidator;

        public OBAuthenticationStateProvider(ILocalStorageService localStorage, OpenBulletSettingsService settings,
            IGuestRepository guestRepo, JwtValidationService jwtValidator)
        {
            this.localStorage = localStorage;
            this.settingsService = settings;
            this.guestRepo = guestRepo;
            this.jwtValidator = jwtValidator;
        }

        /// <inheritdoc/>
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // If we didn't enable admin login, always return an authenticated admin user
            if (!settingsService.Settings.SecuritySettings.RequireAdminLogin)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, settingsService.Settings.SecuritySettings.AdminUsername),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, "noAuth");
                var admin = new ClaimsPrincipal(identity);
                return new AuthenticationState(admin);
            }

            var jwt = await localStorage.GetItemAsync<string>("jwt");

            try 
            {
                var authenticatedUser = jwtValidator.ValidateToken(jwt, out SecurityToken validatedToken);
                return new AuthenticationState(authenticatedUser);
            }
            catch
            {
                // Token not present, expired or forged
                var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity(new[] 
                {
                    new Claim(ClaimTypes.Name, "anonymous"),
                    new Claim(ClaimTypes.Role, "Anonymous")
                }, "anonymous"));
                return new AuthenticationState(anonymousUser);
            }
        }

        /// <summary>
        /// Gets the current user ID. The admin user always has an ID equal to 0, while
        /// guests have sequential user IDs starting from 1. If no user is found, this method returns -1.
        /// </summary>
        public async Task<int> GetCurrentUserId()
        {
            var user = await GetAuthenticationStateAsync();
            var claims = user.User.Claims;
            var role = claims.First(c => c.Type == ClaimTypes.Role).Value;
            var username = claims.First(c => c.Type == ClaimTypes.Name).Value;

            if (role == "Admin")
            {
                return 0;
            }

            if (role == "Guest")
            {
                var guest = await guestRepo.GetAll().FirstOrDefaultAsync(g => g.Username == username);

                if (guest is not null)
                {
                    return guest.Id;
                }
            }

            return -1;
        }

        /// <summary>
        /// Authenticates a user by <paramref name="username"/>, <paramref name="password"/> and <paramref name="ip"/>.
        /// </summary>
        public async Task AuthenticateUser(string username, string password, IPAddress ip)
        {
            if (settingsService.Settings.SecuritySettings.AdminUsername == username)
            {
                await AuthenticateAdmin(username, password);
            }
            else
            {
                await AuthenticateGuest(username, password, ip);
            }
        }

        private async Task AuthenticateAdmin(string username, string password)
        {
            if (!BCrypt.Net.BCrypt.Verify(password, settingsService.Settings.SecuritySettings.AdminPasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid password");
            }

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

        private async Task AuthenticateGuest(string username, string password, IPAddress ip)
        {
            var entity = guestRepo.GetAll().FirstOrDefault(g => g.Username == username);

            if (entity == null)
            {
                throw new EntryNotFoundException("Could not find a guest with the given username");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid password");
            }

            if (DateTime.UtcNow > entity.AccessExpiration)
            {
                throw new UnauthorizedAccessException("Access to this guest account has expired");
            }

            if (ip.IsIPv4MappedToIPv6)
            {
                ip = ip.MapToIPv4();
            }

            var isValid = await Firewall.CheckIpValidity(ip, entity.AllowedAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries));

            if (entity.AllowedAddresses.Length > 0 && !isValid)
            {
                throw new UnauthorizedAccessException($"Unauthorized IP address: {ip}");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, "Guest"),
                new Claim("IPAtLogin", ip.ToString())
            };

            var jwt = BuildGuestToken(claims);
            await localStorage.SetItemAsync("jwt", jwt);

            var identity = new ClaimsIdentity(claims, "guestAuth");
            var guest = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(guest)));
        }

        private string BuildAdminToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(settingsService.Settings.SecuritySettings.JwtKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var hours = Math.Clamp(settingsService.Settings.SecuritySettings.AdminSessionLifetimeHours, 0, 9999);
            var expiration = DateTime.UtcNow.AddHours(hours);

            var token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string BuildGuestToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(settingsService.Settings.SecuritySettings.JwtKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var hours = Math.Clamp(settingsService.Settings.SecuritySettings.GuestSessionLifetimeHours, 0, 9999);
            var expiration = DateTime.UtcNow.AddHours(hours);

            var token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Deletes the JWT from the browser's localStorage.
        /// </summary>
        /// <returns></returns>
        public async Task Logout() => await localStorage.RemoveItemAsync("jwt");
    }
}
