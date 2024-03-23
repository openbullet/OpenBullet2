using Microsoft.IdentityModel.Tokens;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenBullet2.Web.Services;

/// <summary>
/// A JWT-based auth token service.
/// </summary>
public class AuthTokenService : IAuthTokenService
{
    private readonly OpenBulletSettingsService _obSettingsService;

    /// <inheritdoc />
    public AuthTokenService(OpenBulletSettingsService obSettingsService)
    {
        _obSettingsService = obSettingsService;
    }

    /// <inheritdoc />
    public string GenerateToken(IEnumerable<Claim> claims, TimeSpan lifetime)
    {
        var key = new SymmetricSecurityKey(_obSettingsService.Settings.SecuritySettings.JwtKey);
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiration = DateTime.UtcNow + lifetime;

        var token = new JwtSecurityToken(null, null, claims, DateTime.UtcNow, expiration, creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public JwtSecurityToken ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(_obSettingsService.Settings.SecuritySettings.JwtKey);
        var handler = new JwtSecurityTokenHandler();
        var handlerParams = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            handler.ValidateToken(token, handlerParams, out var validToken);

            return (JwtSecurityToken)validToken;
        }
        catch
        {
            throw new UnauthorizedException(ErrorCode.InvalidAuthToken,
                "Invalid authentication token");
        }
    }
}
