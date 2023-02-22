using Microsoft.AspNetCore.SignalR;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Identity;
using System.IdentityModel.Tokens.Jwt;

namespace OpenBullet2.Web.SignalR;

/// <summary>
/// Hub that checks the authorization.
/// </summary>
public abstract class AuthorizedHub : Hub
{
    private readonly IAuthTokenService _tokenService;

    /// <summary>
    /// The verified user.
    /// </summary>
    protected ApiUser? User { get; private set; }

    /// <summary>
    /// Whether this hub should only be used by the admin user.
    /// </summary>
    public bool OnlyAdmin { get; } = false;

    /// <summary></summary>
    public AuthorizedHub(IAuthTokenService tokenService, bool onlyAdmin)
    {
        _tokenService = tokenService;
        OnlyAdmin = onlyAdmin;
    }

    /// <inheritdoc/>
    public override Task OnConnectedAsync()
    {
        // Make sure the user provided a valid auth token
        var request = Context.GetHttpContext()!.Request;
        var authHeader = request.Headers["Authorization"].FirstOrDefault();

        if (authHeader is null)
        {
            throw new UnauthorizedAccessException("Missing auth token");
        }

        var token = authHeader.Split(' ').Last();
        var validToken = _tokenService.ValidateToken(token);
        User = ApiUser.FromToken(validToken);

        if (OnlyAdmin && User.Role is not UserRole.Admin)
        {
            throw new UnauthorizedAccessException("You must be an admin to use this hub");
        }

        return Task.CompletedTask;
    }
}
