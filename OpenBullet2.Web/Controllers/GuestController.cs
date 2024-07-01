using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Dtos.Guest;
using OpenBullet2.Web.Exceptions;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage guest users.
/// </summary>
[TypeFilter<AdminFilter>]
[ApiVersion("1.0")]
public class GuestController : ApiController
{
    private readonly IGuestRepository _guestRepo;
    private readonly ILogger<GuestController> _logger;
    private readonly IMapper _mapper;
    private readonly OpenBulletSettingsService _obSettingsService;
    
    /// <summary></summary>
    public GuestController(IGuestRepository guestRepo, IMapper mapper,
        OpenBulletSettingsService obSettingsService,
        ILogger<GuestController> logger)
    {
        _guestRepo = guestRepo;
        _mapper = mapper;
        _obSettingsService = obSettingsService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new guest user.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> Create(CreateGuestDto dto,
        [FromServices] IValidator<CreateGuestDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        var existing = await _guestRepo.GetAll()
            .FirstOrDefaultAsync(g => g.Username == dto.Username);

        if (existing is not null)
        {
            throw new BadRequestException(ErrorCode.UsernameTaken,
                $"A guest user with the username {dto.Username} already exists");
        }

        // Also make sure the admin is not using the same username
        if (_obSettingsService.Settings.SecuritySettings.AdminUsername == dto.Username)
        {
            throw new BadRequestException(ErrorCode.UsernameTaken,
                "The admin user is already using this username");
        }

        var entity = _mapper.Map<GuestEntity>(dto);
        await _guestRepo.AddAsync(entity);

        _logger.LogInformation("Created a new guest user with username {Username}",
            dto.Username);

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// Update the info of a guest user.
    /// </summary>
    [HttpPatch("info")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> UpdateInfo(UpdateGuestInfoDto dto,
        [FromServices] IValidator<UpdateGuestInfoDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        var entity = await GetEntityAsync(dto.Id);

        // If the username was changed, make sure it's not taken
        if (entity.Username != dto.Username)
        {
            var existing = await _guestRepo.GetAll()
                .FirstOrDefaultAsync(g => g.Username == dto.Username);

            if (existing is not null)
            {
                throw new BadRequestException(ErrorCode.UsernameTaken,
                    $"A guest user with the username {dto.Username} already exists");
            }

            // Also make sure the admin is not using the same username
            if (_obSettingsService.Settings.SecuritySettings.AdminUsername == dto.Username)
            {
                throw new BadRequestException(ErrorCode.UsernameTaken,
                    "The admin user is already using this username");
            }
        }

        _mapper.Map(dto, entity);
        await _guestRepo.UpdateAsync(entity);

        _logger.LogInformation(
            "Updated the information of guest user with username {Username}",
            dto.Username);

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// Update the password of a guest user.
    /// </summary>
    [HttpPatch("password")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> UpdatePassword(UpdateGuestPasswordDto dto,
        [FromServices] IValidator<UpdateGuestPasswordDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        var entity = await GetEntityAsync(dto.Id);

        _mapper.Map(dto, entity);
        await _guestRepo.UpdateAsync(entity);

        _logger.LogInformation(
            "Updated the password of guest user with username {Username}",
            entity.Username);

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// List all the guest users.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<GuestDto>>> GetAll()
    {
        var entities = await _guestRepo.GetAll().ToListAsync();
        return Ok(_mapper.Map<IEnumerable<GuestDto>>(entities));
    }

    /// <summary>
    /// Get the information of a guest user given its id.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> Get(int id)
    {
        var entity = await GetEntityAsync(id);

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// Delete a guest user given its id.
    /// </summary>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(int id)
    {
        var entity = await GetEntityAsync(id);

        await _guestRepo.DeleteAsync(entity);

        _logger.LogInformation("Deleted the guest user with username {Username}",
            entity.Username);

        return Ok();
    }

    private async Task<GuestEntity> GetEntityAsync(int id)
    {
        var entity = await _guestRepo.GetAsync(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.GuestUserNotFound,
                id, nameof(IGuestRepository));
        }

        return entity;
    }
}
