using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.User;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using System.Net;
using System.Security.Claims;
using FluentValidation;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage user sessions.
/// </summary>
[ApiVersion("1.0")]
public class UserController : ApiController
{
    private readonly IAuthTokenService _authService;
    private readonly IGuestRepository _guestRepo;
    private readonly ILogger<UserController> _logger;
    private readonly OpenBulletSettingsService _obSettingsService;

    /// <summary></summary>
    public UserController(OpenBulletSettingsService obSettingsService,
        IAuthTokenService authService, IGuestRepository guestRepo,
        ILogger<UserController> logger)
    {
        _obSettingsService = obSettingsService;
        _authService = authService;
        _guestRepo = guestRepo;
        _logger = logger;
    }

    /// <summary>
    /// Log in as an admin or guest user and get an
    /// access token that can be used for authenticated requests.
    /// </summary>
    [HttpPost("login")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<LoggedInUserDto>> Login(UserLoginDto dto,
        [FromServices] IValidator<UserLoginDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        // Admin user
        if (string.Equals(_obSettingsService.Settings.SecuritySettings.AdminUsername, dto.Username,
                StringComparison.CurrentCultureIgnoreCase))
        {
            return await LoginAdminUser(dto);
        }

        // Guest
        return await LoginGuestUser(dto);
    }

    private Task<LoggedInUserDto> LoginAdminUser(UserLoginDto dto)
    {
        var passwordRequired = _obSettingsService.Settings.SecuritySettings.RequireAdminLogin;
        var passwordHash = _obSettingsService.Settings.SecuritySettings.AdminPasswordHash;
        var validPassword = BCrypt.Net.BCrypt.Verify(dto.Password, passwordHash);

        if (passwordRequired && !validPassword)
        {
            throw new UnauthorizedException(ErrorCode.InvalidCredentials,
                "Invalid username or password");
        }

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, "0", ClaimValueTypes.Integer),
            new Claim(ClaimTypes.Name, dto.Username), new Claim(ClaimTypes.Role, "Admin")
        };

        var lifetimeHours = Math.Clamp(_obSettingsService.Settings.SecuritySettings.AdminSessionLifetimeHours, 0, 9999);
        var token = _authService.GenerateToken(claims, TimeSpan.FromHours(lifetimeHours));
        
        _logger.LogInformation("Admin user logged in");
        
        return Task.FromResult(new LoggedInUserDto { Token = token });
    }

    private async Task<LoggedInUserDto> LoginGuestUser(UserLoginDto dto)
    {
        var entity = await _guestRepo.GetAll().FirstOrDefaultAsync(g => g.Username.ToLower() == dto.Username.ToLower());

        if (entity == null)
        {
            // Invalid username
            throw new UnauthorizedException(ErrorCode.InvalidCredentials,
                "Invalid username or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, entity.PasswordHash))
        {
            throw new UnauthorizedException(ErrorCode.InvalidCredentials,
                "Invalid username or password");
        }

        if (DateTime.UtcNow > entity.AccessExpiration)
        {
            throw new UnauthorizedException(ErrorCode.GuestAccountExpired,
                "Access to this guest account has expired");
        }

        var ip = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;

        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }

        if (entity.AllowedAddresses.Length > 0)
        {
            var isValid = await Firewall.CheckIpValidityAsync(ip,
                entity.AllowedAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries));

            if (!isValid)
            {
                throw new UnauthorizedException(ErrorCode.UnauthorizedIpAddress,
                    $"Unauthorized IP address: {ip}");
            }
        }

        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString(), ClaimValueTypes.Integer),
            new Claim(ClaimTypes.Name, dto.Username), new Claim(ClaimTypes.Role, "Guest"),
            new Claim("IPAtLogin", ip.ToString())
        };

        // Expire the token at the earliest of the two:
        // the expiration of the access or the configured lifetime of the guest token
        var lifetimeHours = Math.Clamp(_obSettingsService.Settings.SecuritySettings.GuestSessionLifetimeHours, 0, 9999);
        var lifetimeSpan = TimeSpan.FromHours(lifetimeHours);
        var accessExpiration = entity.AccessExpiration - DateTime.UtcNow;
        var token = _authService.GenerateToken(claims,
            accessExpiration < lifetimeSpan ? accessExpiration : lifetimeSpan);

        _logger.LogInformation("Guest user {Username} logged in", dto.Username);
        
        return new LoggedInUserDto { Token = token };
    }
}
