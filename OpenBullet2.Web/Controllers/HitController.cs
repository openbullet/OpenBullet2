using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Dtos.User;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using OpenBullet2.Web.Models.Pagination;
using RuriLib.Extensions;
using System.Text;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage hits.
/// </summary>
[Guest]
[ApiVersion("1.0")]
public class HitController : ApiController
{
    private readonly IHitRepository _hitRepo;
    private readonly IGuestRepository _guestRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<HitController> _logger;
    private readonly OpenBulletSettingsService _obSettingsService;

    /// <summary></summary>
    public HitController(IHitRepository hitRepo, IGuestRepository guestRepo,
        IMapper mapper, ILogger<HitController> logger,
        OpenBulletSettingsService obSettingsService)
    {
        _hitRepo = hitRepo;
        _guestRepo = guestRepo;
        _mapper = mapper;
        _logger = logger;
        _obSettingsService = obSettingsService;
    }

    /// <summary>
    /// Create a new hit.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<HitDto>> Create(CreateHitDto dto)
    {
        var apiUser = HttpContext.GetApiUser();

        var entity = _mapper.Map<HitEntity>(dto);

        // If the user is a guest, we also have to mark them as the
        // owner of the hit, for access control purposes.
        if (apiUser.Role is UserRole.Guest)
        {
            entity.OwnerId = apiUser.Id;
        }

        await _hitRepo.Add(entity);

        _logger.LogInformation("Created a new hit");

        var hitDto = _mapper.Map<HitDto>(entity);
        var owner = await _guestRepo.Get(apiUser.Id);

        hitDto.Owner = _mapper.Map<OwnerDto>(owner);
        return hitDto;
    }

    /// <summary>
    /// List all of the available hits, supports filtering and pagination.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PagedList<HitDto>>> GetAll(
        [FromQuery] HitFiltersDto dto)
    {
        var query = FilteredQuery(dto);

        var pagedEntities = await PagedList<HitEntity>.CreateAsync(query,
            dto.PageNumber, dto.PageSize);

        return _mapper.Map<PagedList<HitDto>>(pagedEntities);
    }

    /// <summary>
    /// Download a txt file with one hit per line, containing all hits
    /// that match the filters, formatted with the provided format.
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="format">
    /// Check the documentation for a list of valid formats.
    /// </param>
    [HttpGet("download/many")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DownloadMany(
        [FromQuery] HitFiltersDto dto, string format = "<DATA> | <CAPTURE>")
    {
        var query = FilteredQuery(dto);
        var hits = await query.ToListAsync();

        var outputHits = string.Join(Environment.NewLine,
            hits.Select(h => FormatHit(h, format)));
        
        var bytes = Encoding.UTF8.GetBytes(outputHits);
        return File(bytes, "text/plain", "hits.txt");
    }

    /// <summary>
    /// Update some fields of a hit.
    /// </summary>
    /// <returns></returns>
    [HttpPatch]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<HitDto>> Update(UpdateHitDto dto)
    {
        var entity = await GetEntityAsync(dto.Id);
        EnsureOwnership(entity);

        _mapper.Map(dto, entity);
        await _hitRepo.Update(entity);

        _logger.LogInformation("Updated the information of hit with id {id}", dto.Id);

        return _mapper.Map<HitDto>(entity);
    }

    /// <summary>
    /// Delete a hit by id.
    /// </summary>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(int id)
    {
        var entity = await GetEntityAsync(id);
        EnsureOwnership(entity);

        await _hitRepo.Delete(entity);

        _logger.LogInformation("Deleted the hit with id {id}", id);

        return Ok();
    }

    /// <summary>
    /// Delete all hits that match the filters.
    /// Returns the number of deleted hits.
    /// </summary>
    [HttpDelete("many")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteMany(
        [FromQuery] HitFiltersDto dto)
    {
        var query = FilteredQuery(dto);

        var toDelete = await query.ToListAsync();

        await _hitRepo.Delete(toDelete);

        _logger.LogInformation("Deleted {hitCount} hits", toDelete.Count);

        return new AffectedEntriesDto
        {
            Count = toDelete.Count
        };
    }

    /// <summary>
    /// Delete duplicate hits. Returns the number of deleted hits.
    /// </summary>
    [HttpDelete("duplicates")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteDuplicates()
    {
        var apiUser = HttpContext.GetApiUser();

        var hits = apiUser.Role is UserRole.Admin
            ? await _hitRepo.GetAll().ToListAsync()
            : await _hitRepo.GetAll()
                .Where(h => h.OwnerId == apiUser.Id)
                .ToListAsync();

        var duplicates = hits
            .GroupBy(h => h.GetHashCode(_obSettingsService.Settings.GeneralSettings.IgnoreWordlistNameOnHitsDedupe))
            .Where(g => g.Count() > 1)
            .SelectMany(g => g.OrderBy(h => h.Date)
            .Reverse().Skip(1)).ToList();

        await _hitRepo.Delete(duplicates);

        _logger.LogInformation("Deleted {hitCount} duplicate hits", duplicates.Count);

        return new AffectedEntriesDto
        {
            Count = duplicates.Count
        };
    }

    /// <summary>
    /// Purge all the hits in the database.
    /// Returns the number of deleted hits.
    /// </summary>
    /// <returns></returns>
    [Admin]
    [HttpDelete("purge")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> Purge()
    {
        _logger.LogWarning("Purging all hits from the database...");

        var count = await _hitRepo.Count();
        await _hitRepo.Purge();

        _logger.LogWarning("Purged {count} hits from the database!", count);

        return new AffectedEntriesDto
        {
            Count = count
        };
    }

    private IQueryable<HitEntity> FilteredQuery(HitFiltersDto dto)
    {
        var apiUser = HttpContext.GetApiUser();

        var query = apiUser.Role is UserRole.Admin
            ? _hitRepo.GetAll()
            : _hitRepo.GetAll()
                .Where(h => h.OwnerId == apiUser.Id);

        if (!string.IsNullOrEmpty(dto.SearchTerm))
        {
            query = query.Where(h =>
                EF.Functions.Like(h.Data, $"%{dto.SearchTerm}%") ||
                EF.Functions.Like(h.CapturedData, $"%{dto.SearchTerm}%") ||
                EF.Functions.Like(h.Proxy, $"%{dto.SearchTerm}%") ||
                EF.Functions.Like(h.ConfigName, $"%{dto.SearchTerm}%") ||
                EF.Functions.Like(h.WordlistName, $"%{dto.SearchTerm}%"));
        }

        if (dto.Type is not null)
        {
            query = query.Where(p => p.Type == dto.Type);
        }

        if (dto.MinDate is not null)
        {
            query = query.Where(h => h.Date >= dto.MinDate);
        }

        if (dto.MaxDate is not null)
        {
            query = query.Where(h => h.Date <= dto.MaxDate);
        }

        return query;
    }

    private async Task<HitEntity> GetEntityAsync(int id)
    {
        var entity = await _hitRepo.Get(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.HIT_NOT_FOUND,
                id, nameof(IHitRepository));
        }

        return entity;
    }

    private void EnsureOwnership(HitEntity entity)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is trying to access a hit that is not owned
        // by them, throw a not found exception.
        if (apiUser.Role is UserRole.Guest && apiUser.Id != entity.OwnerId)
        {
            _logger.LogWarning("Guest user {username} tried to access a hit not owned by them",
                apiUser.Username);

            throw new EntryNotFoundException(ErrorCode.HIT_NOT_FOUND,
                entity.Id, nameof(IHitRepository));
        }
    }

    private static string FormatHit(HitEntity hit, string format = "<DATA> | <CAPTURE>")
        => new StringBuilder(format.Unescape())
            .Replace("<DATA>", hit.Data)
            .Replace("<DATE>", hit.Date.ToString())
            .Replace("<CATEGORY>", hit.ConfigCategory)
            .Replace("<CONFIG>", hit.ConfigName)
            .Replace("<PROXY>", hit.Proxy)
            .Replace("<TYPE>", hit.Type)
            .Replace("<WORDLIST>", hit.WordlistName)
            .Replace("<CAPTURE>", hit.CapturedData)
            .ToString();
}
