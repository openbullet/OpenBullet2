using Microsoft.AspNetCore.Mvc.Filters;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Auth;

internal class AdminFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.HttpContext.GetApiUser().Role is not UserRole.Admin)
        {
            throw new ForbiddenException(
                ErrorCode.NotAdmin, "You must be an admin user to perform this operation");
        }
    }
}
