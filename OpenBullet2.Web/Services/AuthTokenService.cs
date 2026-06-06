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
        var handler = CreateTokenHandler();
        var expiration = DateTime.UtcNow + lifetime;
        var header = new JwtHeader(creds);
        var payload = new JwtPayload();

        foreach (var claim in claims)
        {
            AddClaim(payload, claim);
        }

        payload[JwtRegisteredClaimNames.Exp] = EpochTime.GetIntDate(expiration.ToUniversalTime());

        var token = new JwtSecurityToken(header, payload);
        return handler.WriteToken(token);
    }

    /// <inheritdoc />
    public JwtSecurityToken ValidateToken(string token)
    {
        var key = new SymmetricSecurityKey(_obSettingsService.Settings.SecuritySettings.JwtKey);
        var handler = CreateTokenHandler();
        var handlerParams = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            handler.ValidateToken(token, handlerParams, out _);
            return handler.ReadJwtToken(token);
        }
        catch
        {
            throw new UnauthorizedException(ErrorCode.InvalidAuthToken,
                "Invalid authentication token");
        }
    }

    private static JwtSecurityTokenHandler CreateTokenHandler()
    {
        var handler = new JwtSecurityTokenHandler();
        handler.OutboundClaimTypeMap.Clear();
        handler.InboundClaimTypeMap.Clear();
        return handler;
    }

    private static void AddClaim(JwtPayload payload, Claim claim)
    {
        if (payload.TryGetValue(claim.Type, out var existingValue))
        {
            if (existingValue is List<object> values)
            {
                values.Add(claim.Value);
            }
            else
            {
                payload[claim.Type] = new List<object> { existingValue, claim.Value };
            }

            return;
        }

        payload[claim.Type] = claim.Value;
    }
}
