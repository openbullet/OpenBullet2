using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Guest;
using OpenBullet2.Web.Exceptions;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage guest users.
/// </summary>
[Admin]
[ApiVersion("1.0")]
public class GuestController : ApiController
{
    private readonly IGuestRepository _guestRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<GuestController> _logger;

    /// <summary></summary>
    public GuestController(IGuestRepository guestRepo, IMapper mapper,
        ILogger<GuestController> logger)
    {
        _guestRepo = guestRepo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new guest user.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> Create(CreateGuestDto dto)
    {
        var existing = await _guestRepo.GetAll().FirstOrDefaultAsync(g => g.Username == dto.Username);

        if (existing is not null)
        {
            return BadRequest("There is already a guest user with this username");
        }

        var entity = _mapper.Map<GuestEntity>(dto);
        await _guestRepo.Add(entity);

        _logger.LogInformation($"Created a new guest user with username {dto.Username}");

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// Updates the info of a guest user.
    /// </summary>
    [HttpPatch("info")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> UpdateInfo(UpdateGuestInfoDto dto)
    {
        var entity = await _guestRepo.Get(dto.Id);

        if (entity is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.GUEST_NOT_FOUND,
                dto.Id, nameof(IGuestRepository));
        }

        _mapper.Map(dto, entity);
        await _guestRepo.Update(entity);

        _logger.LogInformation($"Updated the information of guest user with username {dto.Username}");

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// Updates the password of a guest user.
    /// </summary>
    [HttpPatch("password")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> UpdatePassword(UpdateGuestPasswordDto dto)
    {
        var entity = await _guestRepo.Get(dto.Id);

        if (entity is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.GUEST_NOT_FOUND,
                dto.Id, nameof(IGuestRepository));
        }

        _mapper.Map(dto, entity);
        await _guestRepo.Update(entity);

        _logger.LogInformation($"Updated the password of guest user with username {entity.Username}");

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// Lists all the guest users.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<GuestDto>>> GetAll()
    {
        var entities = await _guestRepo.GetAll().ToListAsync();
        return Ok(_mapper.Map<IEnumerable<GuestDto>>(entities));
    }

    /// <summary>
    /// Gets the information of a guest user given its id.
    /// </summary>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<GuestDto>> Get(int id)
    {
        var entity = await _guestRepo.Get(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.GUEST_NOT_FOUND,
                id, nameof(IGuestRepository));
        }

        return _mapper.Map<GuestDto>(entity);
    }

    /// <summary>
    /// Deletes a guest user given its id.
    /// </summary>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(int id)
    {
        var entity = await _guestRepo.Get(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.GUEST_NOT_FOUND, 
                id, nameof(IGuestRepository));
        }

        await _guestRepo.Delete(entity);

        _logger.LogInformation($"Deleted the guest user with username {entity.Username}");

        return Ok();
    }
}
