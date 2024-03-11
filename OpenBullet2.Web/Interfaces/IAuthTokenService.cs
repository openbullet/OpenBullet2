using OpenBullet2.Web.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenBullet2.Web.Interfaces;

/// <summary>
/// Service that handles JWT generation and validation.
/// </summary>
public interface IAuthTokenService
{
    /// <summary>
    /// Generates a JWT.
    /// </summary>
    /// <param name="claims">The claims to write inside the token</param>
    /// <param name="lifetime">The amount of time the token is valid for</param>
    string GenerateToken(IEnumerable<Claim> claims, TimeSpan lifetime);

    /// <summary>
    /// Validates a JWT.
    /// </summary>
    /// <param name="token">The JWT to validate</param>
    /// <exception cref="UnauthorizedException">Thrown when the JWT is invalid</exception>
    JwtSecurityToken ValidateToken(string token);
}
