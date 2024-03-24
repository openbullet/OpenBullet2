using OpenBullet2.Core.Services;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Exceptions;

internal class AuthTokenVerificationMiddleware
{
    private readonly IAuthTokenService _authTokenService;
    private readonly RequestDelegate _next;
    private readonly OpenBulletSettingsService _obSettingsService;

    public AuthTokenVerificationMiddleware(RequestDelegate next,
        OpenBulletSettingsService obSettingsService,
        IAuthTokenService authTokenService)
    {
        _next = next;
        _obSettingsService = obSettingsService;
        _authTokenService = authTokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // If the admin user does not need any login, allow anonymous requests
        if (!_obSettingsService.Settings.SecuritySettings.RequireAdminLogin)
        {
            var apiUser = new ApiUser {
                Id = -1, Role = UserRole.Admin, Username = _obSettingsService.Settings.SecuritySettings.AdminUsername
            };

            context.SetApiUser(apiUser);

            await _next(context);

            return;
        }
        
        // If the user provided the X-Api-Key header, validate it.
        // For now, only the admin can use the API
        var apiKey = context.Request.Headers["X-Api-Key"].FirstOrDefault();
        if (apiKey is not null)
        {
            var validApiKey = _obSettingsService.Settings.SecuritySettings.AdminApiKey;
            if (apiKey == validApiKey)
            {
                var apiUser = new ApiUser {
                    Id = -1, Role = UserRole.Admin, Username = _obSettingsService.Settings.SecuritySettings.AdminUsername
                };

                context.SetApiUser(apiUser);

                await _next(context);

                return;
            }
        }

        // If there is a JWT, validate it
        var tokenParts = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ");
        if (tokenParts is not ["Bearer", _])
        {
            await _next(context);
            return;
        }

        var token = tokenParts[1];
        var validToken = _authTokenService.ValidateToken(token);
        context.SetApiUser(ApiUser.FromToken(validToken));
        await _next(context);
    }
}
