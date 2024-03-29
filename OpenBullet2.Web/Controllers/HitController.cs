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
using System.Globalization;
using System.Text;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage hits.
/// </summary>
[Guest]
[ApiVersion("1.0")]
public class HitController : ApiController
{
    private readonly IGuestRepository _guestRepo;
    private readonly IHitRepository _hitRepo;
    private readonly ILogger<HitController> _logger;
    private readonly IMapper _mapper;
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

        await _hitRepo.AddAsync(entity);

        _logger.LogInformation("Created a new hit");

        var hitDto = _mapper.Map<HitDto>(entity);
        var owner = await _guestRepo.GetAsync(apiUser.Id);

        hitDto.Owner = _mapper.Map<OwnerDto>(owner);
        return hitDto;
    }

    /// <summary>
    /// List all the available hits, supports filtering and pagination.
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
    /// Get the names of all the configs that have hits in the database.
    /// </summary>
    [HttpGet("config-names")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<string>>> GetConfigNames()
    {
        var apiUser = HttpContext.GetApiUser();

        var query = apiUser.Role is UserRole.Admin
            ? _hitRepo.GetAll()
            : _hitRepo.GetAll()
                .Where(h => h.OwnerId == apiUser.Id);

        var configNames = await query
            .Select(h => h.ConfigName)
            .Distinct()
            .ToListAsync();

        return configNames;
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
        await _hitRepo.UpdateAsync(entity);

        _logger.LogInformation("Updated the information of hit with id {Id}", dto.Id);

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

        await _hitRepo.DeleteAsync(entity);

        _logger.LogInformation("Deleted the hit with id {Id}", id);

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

        await _hitRepo.DeleteAsync(toDelete);

        _logger.LogInformation("Deleted {HitCount} hits", toDelete.Count);

        return new AffectedEntriesDto { Count = toDelete.Count };
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

        await _hitRepo.DeleteAsync(duplicates);

        _logger.LogInformation("Deleted {HitCount} duplicate hits", duplicates.Count);

        return new AffectedEntriesDto { Count = duplicates.Count };
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

        var count = await _hitRepo.CountAsync();
        await _hitRepo.PurgeAsync();

        _logger.LogWarning("Purged {Count} hits from the database!", count);

        return new AffectedEntriesDto { Count = count };
    }

    /// <summary>
    /// Get stats about recent hits.
    /// </summary>
    [HttpGet("recent")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<RecentHitsDto>> GetRecent(int days)
    {
        var apiUser = HttpContext.GetApiUser();

        var query = apiUser.Role is UserRole.Admin
            ? _hitRepo.GetAll()
                .Where(h => h.Type == "SUCCESS")
            : _hitRepo.GetAll()
                .Where(h => h.OwnerId == apiUser.Id)
                .Where(h => h.Type == "SUCCESS");

        var dates = Enumerable.Range(0, days)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .Reverse()
            .ToList();

        // First of all, get the distinct names of the configs
        // that have hits in the last N days
        var configNames = await query
            .Where(h => h.Date >= DateTime.UtcNow.Date.AddDays(-days))
            .Select(h => h.ConfigName)
            .Distinct()
            .ToListAsync();

        // For each config, get the number of hits for each day
        var hits = new Dictionary<string, List<int>>();

        foreach (var configName in configNames)
        {
            var configHits = new List<int>();

            foreach (var date in dates)
            {
                var dailyHits = await query
                    .Where(h => h.ConfigName == configName && h.Date.Date == date)
                    .CountAsync();

                configHits.Add(dailyHits);
            }

            hits.Add(configName, configHits);
        }

        // If there are no hits, return an empty response
        if (hits.Count == 0)
        {
            return new RecentHitsDto();
        }

        return new RecentHitsDto { Dates = dates, Hits = hits };
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
                EF.Functions.Like(h.WordlistName, $"%{dto.SearchTerm}%"));
        }
        
        if (!string.IsNullOrEmpty(dto.ConfigName))
        {
            query = query.Where(h => h.ConfigName == dto.ConfigName);
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

        if (dto.SortBy is not null)
        {
            switch (dto.SortBy)
            {
                case HitSortField.Date:
                    query = dto.SortDescending
                        ? query.OrderByDescending(h => h.Date)
                        : query.OrderBy(h => h.Date);
                    break;
            }
        }

        return query;
    }

    private async Task<HitEntity> GetEntityAsync(int id)
    {
        var entity = await _hitRepo.GetAsync(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.HitNotFound,
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
            _logger.LogWarning("Guest user {Username} tried to access a hit not owned by them",
                apiUser.Username);

            throw new EntryNotFoundException(ErrorCode.HitNotFound,
                entity.Id, nameof(IHitRepository));
        }
    }

    private static string FormatHit(HitEntity hit, string format = "<DATA> | <CAPTURE>")
        => new StringBuilder(format.Unescape())
            .Replace("<DATA>", hit.Data)
            .Replace("<DATE>", hit.Date.ToString(CultureInfo.InvariantCulture))
            .Replace("<CATEGORY>", hit.ConfigCategory)
            .Replace("<CONFIG>", hit.ConfigName)
            .Replace("<PROXY>", hit.Proxy)
            .Replace("<TYPE>", hit.Type)
            .Replace("<WORDLIST>", hit.WordlistName)
            .Replace("<CAPTURE>", hit.CapturedData)
            .ToString();
}
