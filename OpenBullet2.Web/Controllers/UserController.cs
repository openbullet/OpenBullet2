using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.User;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Interfaces;
using System.Security.Claims;

namespace OpenBullet2.Web.Controllers;

public class UserController : ApiController
{
    private readonly OpenBulletSettingsService _obSettingsService;
    private readonly IAuthTokenService _authService;
    private readonly IGuestRepository _guestRepo;

    public UserController(OpenBulletSettingsService obSettingsService,
        IAuthTokenService authService, IGuestRepository guestRepo)
    {
        _obSettingsService = obSettingsService;
        _authService = authService;
        _guestRepo = guestRepo;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoggedInUserDto>> Login(UserLoginDto dto)
    {
        // Admin user
        if (_obSettingsService.Settings.SecuritySettings.AdminUsername == dto.Username)
        {
            return await LoginAdminUser(dto);
        }
        // Guest
        else
        {
            return await LoginGuestUser(dto);
        }
    }

    private Task<LoggedInUserDto> LoginAdminUser(UserLoginDto dto)
    {
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, _obSettingsService.Settings.SecuritySettings.AdminPasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        var claims = new[]
        {
                new Claim(ClaimTypes.Name, dto.Username),
                new Claim(ClaimTypes.Role, "Admin")
            };

        var lifetimeHours = Math.Clamp(_obSettingsService.Settings.SecuritySettings.AdminSessionLifetimeHours, 0, 9999);
        var token = _authService.GenerateToken(claims, TimeSpan.FromHours(lifetimeHours));

        return Task.FromResult(new LoggedInUserDto
        {
            Token = token
        });
    }

    private async Task<LoggedInUserDto> LoginGuestUser(UserLoginDto dto)
    {
        var entity = await _guestRepo.GetAll().FirstOrDefaultAsync(g => g.Username == dto.Username);

        if (entity == null)
        {
            throw new EntryNotFoundException(
                ErrorCode.GUEST_NOT_FOUND,
                "Could not find a guest with the given username");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, entity.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid password");
        }

        if (DateTime.UtcNow > entity.AccessExpiration)
        {
            throw new UnauthorizedAccessException("Access to this guest account has expired");
        }

        var ip = HttpContext.Connection.RemoteIpAddress;

        if (ip is null)
        {
            throw new UnauthorizedAccessException("Failed to read the IP of the calling client");
        }

        if (ip.IsIPv4MappedToIPv6)
        {
            ip = ip.MapToIPv4();
        }
        
        if (entity.AllowedAddresses.Length > 0)
        {
            var isValid = await Firewall.CheckIpValidity(ip, entity.AllowedAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries));

            if (!isValid)
            {
                throw new UnauthorizedAccessException($"Unauthorized IP address: {ip}");
            }
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, dto.Username),
            new Claim(ClaimTypes.Role, "Guest"),
            new Claim("IPAtLogin", ip.ToString())
        };

        var lifetimeHours = Math.Clamp(_obSettingsService.Settings.SecuritySettings.GuestSessionLifetimeHours, 0, 9999);
        var token = _authService.GenerateToken(claims, TimeSpan.FromHours(lifetimeHours));

        return new LoggedInUserDto
        {
            Token = token
        };
    }
}
