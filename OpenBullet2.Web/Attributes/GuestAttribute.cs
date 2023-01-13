using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace OpenBullet2.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class GuestAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authToken = (JwtSecurityToken?)context.HttpContext.Items["authToken"];

        if (authToken is null)
        {
            throw new UnauthorizedAccessException("Auth token not provided");
        }

        var role = authToken.Claims.FirstOrDefault(
            c => c.Type == ClaimTypes.Role || c.Type == "role")?.Value;

        if (role != "Guest" && role != "Admin")
        {
            throw new UnauthorizedAccessException(
                "You must be a guest or admin user to perform this operation");
        }
    }
}
