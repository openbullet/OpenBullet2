using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// Hub that checks the authorization.
/// </summary>
public abstract class AuthorizedHub : Hub
{
    private readonly OpenBulletSettingsService _obSettingsService;
    private readonly IAuthTokenService _tokenService;

    /// <summary></summary>
    protected AuthorizedHub(IAuthTokenService tokenService,
        OpenBulletSettingsService obSettingsService, bool onlyAdmin)
    {
        _tokenService = tokenService;
        _obSettingsService = obSettingsService;
        OnlyAdmin = onlyAdmin;
    }

    /// <summary>
    /// The verified user.
    /// </summary>
    private ApiUser? User { get; set; }

    /// <summary>
    /// Whether this hub should only be used by the admin user.
    /// </summary>
    private bool OnlyAdmin { get; }

    /// <inheritdoc />
    public override async Task OnConnectedAsync()
    {
        // If the admin user does not need any login, allow anonymous requests
        if (!_obSettingsService.Settings.SecuritySettings.RequireAdminLogin)
        {
            User = new ApiUser {
                Id = -1, Role = UserRole.Admin, Username = _obSettingsService.Settings.SecuritySettings.AdminUsername
            };

            return;
        }

        // Make sure the user provided a valid auth token
        var request = Context.GetHttpContext()!.Request;
        var accessToken = request.Query["access_token"].FirstOrDefault();

        if (accessToken is null)
        {
            await Clients.Caller.SendAsync(
                CommonMethods.Error,
                new ErrorMessage { Message = "Missing auth token", Type = nameof(UnauthorizedException) });

            throw new UnauthorizedException(
                ErrorCode.MissingAuthToken, "Missing auth token");
        }

        try
        {
            var validToken = _tokenService.ValidateToken(accessToken);
            User = ApiUser.FromToken(validToken);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync(
                CommonMethods.Error,
                new ErrorMessage { Message = ex.Message, Type = ex.GetType().Name });

            throw;
        }

        if (OnlyAdmin && User.Role is not UserRole.Admin)
        {
            await Clients.Caller.SendAsync(
                CommonMethods.Error,
                new ErrorMessage {
                    Message = "You must be an admin to use this hub", Type = nameof(UnauthorizedException)
                });

            throw new UnauthorizedException(ErrorCode.NotAdmin,
                "You must be an admin to use this hub");
        }
    }
}
