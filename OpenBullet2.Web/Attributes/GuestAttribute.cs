using Microsoft.AspNetCore.Mvc.Filters;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal class GuestAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.GetApiUser().Role is not UserRole.Admin
            and not UserRole.Guest)
        {
            throw new UnauthorizedAccessException(
                "You must be a guest or admin user to perform this operation");
        }
    }
}   
