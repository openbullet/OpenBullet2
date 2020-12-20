using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenBullet2.Exceptions;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenBullet2.Auth
{
    public class OBAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService localStorage;
        private readonly PersistentSettingsService settings;
        private readonly IGuestRepository guestRepo;
        private readonly JwtValidationService jwtValidator;

        public OBAuthenticationStateProvider(ILocalStorageService localStorage, PersistentSettingsService settings,
            IGuestRepository guestRepo, JwtValidationService jwtValidator)
        {
            this.localStorage = localStorage;
            this.settings = settings;
            this.guestRepo = guestRepo;
            this.jwtValidator = jwtValidator;
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

        public async Task<int> GetCurrentUserId()
        {
            var user = await GetAuthenticationStateAsync();
            var claims = user.User.Claims;
            var role = claims.First(c => c.Type == ClaimTypes.Role).Value;
            var username = claims.First(c => c.Type == ClaimTypes.Name).Value;

            return role switch
            {
                "Admin" => 0,
                "Guest" => (await guestRepo.GetAll().FirstOrDefaultAsync(g => g.Username == username)).Id,
                _ => -1
            };
        }

        public async Task AuthenticateUser(string username, string password, IPAddress ip)
        {
            if (settings.OpenBulletSettings.SecuritySettings.AdminUsername == username)
                await AuthenticateAdmin(username, password);
            else
                await AuthenticateGuest(username, password, ip);
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

        private async Task AuthenticateGuest(string username, string password, IPAddress ip)
        {
            var entity = guestRepo.GetAll().FirstOrDefault(g => g.Username == username);

            if (entity == null)
                throw new EntryNotFoundException("Could not find a guest with the given username");

            if (!BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash))
                throw new UnauthorizedAccessException("Invalid password");

            if (DateTime.UtcNow > entity.AccessExpiration)
                throw new UnauthorizedAccessException("Access to this guest account has expired");

            if (ip.IsIPv4MappedToIPv6)
                ip = ip.MapToIPv4();

            if (entity.AllowedAddresses.Count() > 0 && !CheckIpValidity(ip, entity.AllowedAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries)))
                throw new UnauthorizedAccessException($"Unauthorized IP address: {ip}");

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
            var key = new SymmetricSecurityKey(settings.OpenBulletSettings.SecuritySettings.JwtKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(settings.OpenBulletSettings.SecuritySettings.AdminSessionLifetimeHours);

            JwtSecurityToken token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string BuildGuestToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(settings.OpenBulletSettings.SecuritySettings.JwtKey);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(settings.OpenBulletSettings.SecuritySettings.GuestSessionLifetimeHours);

            JwtSecurityToken token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task Logout()
        {
            await localStorage.RemoveItemAsync("jwt");
        }

        // Supported: IPv4, IPv6, masked IPv4, dynamic DNS
        private bool CheckIpValidity(IPAddress ip, IEnumerable<string> allowed)
        {
            foreach (var addr in allowed)
            {
                try
                {
                    // Check if standard IPv4 or IPv6
                    if (Regex.Match(addr, @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\.|$)){4}$").Success ||
                        Regex.Match(addr, @"^(([0-9a-fA-F]{1,4}:){7,7}[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,7}:|([0-9a-fA-F]{1,4}:){1,6}:[0-9a-fA-F]{1,4}|([0-9a-fA-F]{1,4}:){1,5}(:[0-9a-fA-F]{1,4}){1,2}|([0-9a-fA-F]{1,4}:){1,4}(:[0-9a-fA-F]{1,4}){1,3}|([0-9a-fA-F]{1,4}:){1,3}(:[0-9a-fA-F]{1,4}){1,4}|([0-9a-fA-F]{1,4}:){1,2}(:[0-9a-fA-F]{1,4}){1,5}|[0-9a-fA-F]{1,4}:((:[0-9a-fA-F]{1,4}){1,6})|:((:[0-9a-fA-F]{1,4}){1,7}|:)|fe80:(:[0-9a-fA-F]{0,4}){0,4}%[0-9a-zA-Z]{1,}|::(ffff(:0{1,4}){0,1}:){0,1}((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])|([0-9a-fA-F]{1,4}:){1,4}:((25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9])\.){3,3}(25[0-5]|(2[0-4]|1{0,1}[0-9]){0,1}[0-9]))$").Success)
                    {
                        if (ip.Equals(IPAddress.Parse(addr)))
                            return true;
                    }

                    // Check if masked IPv4
                    if (addr.Contains('/'))
                    {
                        var split = addr.Split('/');
                        var maskLength = int.Parse(split[1]);
                        var toCompare = IPAddress.Parse(split[0]);
                        var mask = SubnetMask.CreateByNetBitLength(maskLength);

                        if (ip.IsInSameSubnet(toCompare, mask))
                            return true;
                    }

                    // Otherwise it must be a dynamic DNS
                    var resolved = Dns.GetHostEntry(addr);
                    if (resolved.AddressList.Any(a => a.Equals(ip)))
                        return true;
                }
                catch
                {

                }
            }

            return false;
        }
    }
}
