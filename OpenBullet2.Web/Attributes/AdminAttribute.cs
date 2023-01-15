using Microsoft.AspNetCore.Mvc.Filters;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace OpenBullet2.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class AdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.GetApiUser().Role is not UserRole.Admin)
        {
            throw new UnauthorizedAccessException(
                "You must be an admin user to perform this operation");
        }
    }
}
