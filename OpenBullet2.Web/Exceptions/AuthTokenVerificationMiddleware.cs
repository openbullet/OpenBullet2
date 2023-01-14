using OpenBullet2.Web.Interfaces;

namespace OpenBullet2.Web.Exceptions;

internal class AuthTokenVerificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthTokenVerificationMiddleware> _logger;
    private readonly IAuthTokenService _authTokenService;

    public AuthTokenVerificationMiddleware(RequestDelegate next,
        ILogger<AuthTokenVerificationMiddleware> logger,
        IAuthTokenService authTokenService)
    {
        _next = next;
        _logger = logger;
        _authTokenService = authTokenService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // If there is a token, validate it
        var token = context.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(' ').Last();

        if (token is not null)
        {
            context.Items["authToken"] = _authTokenService.ValidateToken(token);
        }

        await _next(context);
    }
}
