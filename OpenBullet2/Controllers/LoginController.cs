using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using OpenBullet2.DTOs;
using OpenBullet2.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpenBullet2.Controllers
{
    [Route("[controller]/[action]")]
    public class LoginController : Controller
    {
        private readonly PersistentSettingsService settings;

        public LoginController(PersistentSettingsService settings)
        {
            this.settings = settings;
            // HERE INJECT THE USERS TABLE
        }

        [HttpPost]
        public IActionResult AdminLogin(string password, string redirectUri)
        {
            if (BCrypt.Net.BCrypt.Verify(password, settings.SecurityOptions.AdminPasswordHash))
            {
                var token = BuildAdminToken();
                HttpContext.Response.Cookies.Append("jwt", token.Jwt, new CookieOptions { Expires = token.Expiration });
                return LocalRedirect(redirectUri);
            }
            else
            {
                return BadRequest("Invalid password");
            }
        }

        private UserToken BuildAdminToken()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "Admin"),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecurityOptions.JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(1);

            JwtSecurityToken token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
            return new UserToken
            {
                Jwt = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

        /*
        public IActionResult GuestLogin(string username, string password)
        {
            HttpContext.Response.Cookies.Append("jwt", );

            return LocalRedirect(redirectUri);
        }
        */
    }
}
