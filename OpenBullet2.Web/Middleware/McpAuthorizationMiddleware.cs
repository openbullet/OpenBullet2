using OpenBullet2.Core.Services;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;

namespace OpenBullet2.Web.Middleware;

internal class McpAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly OpenBulletSettingsService _obSettingsService;

    public McpAuthorizationMiddleware(RequestDelegate next, OpenBulletSettingsService obSettingsService)
    {
        _next = next;
        _obSettingsService = obSettingsService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/mcp", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (!_obSettingsService.Settings.SecuritySettings.RequireAdminLogin)
        {
            await _next(context);
            return;
        }

        var apiUser = context.GetApiUser();
        if (apiUser.Role is not UserRole.Admin)
        {
            throw new ForbiddenException(
                ErrorCode.NotAdmin, "You must be an admin user to access the MCP server");
        }

        await _next(context);
    }
}
