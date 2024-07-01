using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenBullet2.Web.Models.Identity;

/// <summary>
/// Information of a logged in API user.
/// </summary>
public class ApiUser
{
    /// <summary>
    /// The id of the user.
    /// </summary>
    public int Id { get; set; } = -1;

    /// <summary>
    /// The role of the user.
    /// </summary>
    public UserRole Role { get; set; } = UserRole.Anonymous;

    /// <summary>
    /// The username.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Builds an <see cref="ApiUser" /> from a jwt.
    /// </summary>
    public static ApiUser FromToken(JwtSecurityToken jwt)
        => new() {
            Id = int.Parse(jwt.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameidentifier")?.Value ?? "-1"),
            Username = jwt.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.Name || c.Type == "name")?.Value,
            Role = Enum.Parse<UserRole>(jwt.Claims.FirstOrDefault(
                c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value ?? "Anonymous")
        };
}

/// <summary>
/// The available user roles.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Admin user.
    /// </summary>
    Admin,

    /// <summary>
    /// Guest user.
    /// </summary>
    Guest,

    /// <summary>
    /// Anonymous user.
    /// </summary>
    Anonymous
}
