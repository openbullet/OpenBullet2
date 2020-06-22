using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenBullet2.Services
{
    public class JwtValidationService
    {
        private readonly PersistentSettingsService settings;
        private readonly TokenValidationParameters validationParams;
        private readonly JwtSecurityTokenHandler handler;

        public JwtValidationService(PersistentSettingsService settings)
        {
            this.settings = settings;
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

        public ClaimsPrincipal ValidateToken(string jwt, out SecurityToken validatedToken)
        {
            return handler.ValidateToken(jwt, validationParams, out validatedToken);
        }
    }
}
