using Microsoft.AspNetCore.Mvc.Filters;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Auth;

internal class GuestFilter : IAsyncAuthorizationFilter
{
    private readonly IGuestRepository _guestRepo;

    /// <summary></summary>
    public GuestFilter(IGuestRepository guestRepo)
    {
        _guestRepo = guestRepo;
    }
    
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var apiUser = context.HttpContext.GetApiUser();
        
        if (apiUser.Role is not UserRole.Admin and not UserRole.Guest)
        {
            throw new UnauthorizedException(ErrorCode.InvalidRole,
                "You must be a guest or admin user to perform this operation");
        }

        if (apiUser.Role is UserRole.Guest)
        {
            var guest = await _guestRepo.GetAsync(apiUser.Id);
            
            if (guest is null)
            {
                throw new UnauthorizedException(ErrorCode.InvalidGuestAccount,
                    "The guest user does not exist in the database");
            }
            
            if (guest.AccessExpiration < DateTime.Now)
            {
                throw new UnauthorizedException(ErrorCode.GuestAccountExpired,
                    "Access to the guest user has expired");
            }
        }
    }
}
