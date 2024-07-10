using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Hit;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using OpenBullet2.Web.Models.Pagination;
using RuriLib.Extensions;
using System.Globalization;
using System.Text;
using FluentValidation;
using Newtonsoft.Json;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Web.Auth;
using RuriLib.Services;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage hits.
/// </summary>
[TypeFilter<GuestFilter>]
[ApiVersion("1.0")]
public class HitController : ApiController
{
    private readonly IGuestRepository _guestRepo;
    private readonly IHitRepository _hitRepo;
    private readonly ILogger<HitController> _logger;
    private readonly IMapper _mapper;
    private readonly OpenBulletSettingsService _obSettingsService;
    private readonly RuriLibSettingsService _rlSettingsService;
    private readonly ConfigService _configService;
    private readonly IJobRepository _jobRepo;
    private readonly JobFactoryService _jobFactoryService;
    private readonly JobManagerService _jobManagerService;
    
    /// <summary></summary>
    public HitController(IHitRepository hitRepo, IGuestRepository guestRepo,
        IMapper mapper, ILogger<HitController> logger,
        OpenBulletSettingsService obSettingsService,
        RuriLibSettingsService rlSettingsService,
        ConfigService configService, IJobRepository jobRepo,
        JobFactoryService jobFactoryService, JobManagerService jobManagerService)
    {
        _hitRepo = hitRepo;
        _guestRepo = guestRepo;
        _mapper = mapper;
        _logger = logger;
        _obSettingsService = obSettingsService;
        _rlSettingsService = rlSettingsService;
        _configService = configService;
        _jobRepo = jobRepo;
        _jobFactoryService = jobFactoryService;
        _jobManagerService = jobManagerService;
    }

    /// <summary>
    /// Create a new hit.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<HitDto>> Create(CreateHitDto dto,
        [FromServices] IValidator<CreateHitDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
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
        
        hitDto.OwnerId = owner?.Id ?? 0;
        return hitDto;
    }

    /// <summary>
    /// List all the available hits, supports filtering and pagination.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PagedList<HitDto>>> GetAll(
        [FromQuery] PaginatedHitFiltersDto dto)
    {
        var query = FilteredQuery(_mapper.Map<HitFiltersDto>(dto));

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
    /// Get all hits that match the filters with the provided format.
    /// </summary>
    [HttpGet("formatted/many")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<string>>> FormatMany(
        [FromQuery] HitFiltersDto dto, string format = "<DATA> | <CAPTURE>")
    {
        var query = FilteredQuery(dto);
        var hits = await query.ToListAsync();

        return Ok(hits.Select(h => FormatHit(h, format)));
    }

    /// <summary>
    /// Update some fields of a hit.
    /// </summary>
    /// <returns></returns>
    [HttpPatch]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<HitDto>> Update(UpdateHitDto dto,
        [FromServices] IValidator<UpdateHitDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
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
    [TypeFilter<AdminFilter>]
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
    
    /// <summary>
    /// Send all hits that match the filters to recheck by creating
    /// a temporary file and a MultiRun job. If the hits come from
    /// the same config, the job will use that config.
    /// </summary>
    [HttpPost("send-to-recheck")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<SendToRecheckResultDto>> SendToRecheck(
        [FromBody] HitFiltersDto dto)
    {
        var apiUser = HttpContext.GetApiUser();
        var query = FilteredQuery(dto);
        
        var hits = await query.ToListAsync();
        
        if (hits.Count == 0)
        {
            throw new ApiException(ErrorCode.NoHitsSelected, "No hits selected to recheck");
        }
        
        var jobOptions = new MultiRunJobOptions
        {
            Name = "Recheck"
        };
        var wordlistType = _rlSettingsService.Environment.WordlistTypes[0].Name;
        
        // If all hits come from the same config, use that config
        if (hits.Select(h => h.ConfigId).Distinct().Count() == 1)
        {
            var config = _configService.Configs.Find(c => c.Id == hits[0].ConfigId);
            
            // If we cannot find a config with that id anymore, don't set it
            if (config != null)
            {
                jobOptions.ConfigId = config.Id;
                jobOptions.Bots = config.Settings.GeneralSettings.SuggestedBots;
                
                if (config.Settings.DataSettings.AllowedWordlistTypes.Length > 0)
                {
                    wordlistType = config.Settings.DataSettings.AllowedWordlistTypes[0];
                }
            }
        }
        
        // Write the hits to a temporary file
        var tempFile = Path.GetRandomFileName();
        await System.IO.File.WriteAllLinesAsync(tempFile, hits.Select(h => h.Data));
        jobOptions.DataPool = new FileDataPoolOptions
        {
            FileName = tempFile,
            WordlistType = wordlistType
        };
        jobOptions.HitOutputs.Add(new DatabaseHitOutputOptions());
        
        // Create the job entity and add it to the database
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        var jobOptionsWrapper = new JobOptionsWrapper { Options = jobOptions };
        
        var entity = new JobEntity
        {
            Owner = apiUser.Role is UserRole.Admin ? null : await _guestRepo.GetAsync(apiUser.Id),
            CreationDate = DateTime.Now,
            JobType = JobType.MultiRun,
            JobOptions = JsonConvert.SerializeObject(jobOptionsWrapper, jsonSettings)
        };
        
        await _jobRepo.AddAsync(entity);
        
        var job = _jobFactoryService.FromOptions(entity.Id, apiUser.Id, jobOptions);
        _jobManagerService.AddJob(job);
        
        return new SendToRecheckResultDto { JobId = entity.Id };
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

        if (dto.Types is not null)
        {
            var types = dto.Types.Split(',');
            query = query.Where(h => types.Contains(h.Type));
        }

        if (dto.MinDate is not null)
        {
            // TODO: Save dates in UTC in the database! (evaluate the implications)
            query = query.Where(
                h => h.Date >= TimeZoneInfo.ConvertTimeFromUtc(
                    dto.MinDate.Value, TimeZoneInfo.Local));
        }

        if (dto.MaxDate is not null)
        {
            // TODO: Save dates in UTC in the database! (evaluate the implications)
            query = query.Where(
                h => h.Date <= TimeZoneInfo.ConvertTimeFromUtc(
                    dto.MaxDate.Value, TimeZoneInfo.Local));
        }

        if (dto.SortBy is not null)
        {
            query = dto.SortBy switch
            {
                HitSortField.Type => dto.SortDescending
                    ? query.OrderByDescending(h => h.Type)
                    : query.OrderBy(h => h.Type),
                HitSortField.Data => dto.SortDescending
                    ? query.OrderByDescending(h => h.Data)
                    : query.OrderBy(h => h.Data),
                HitSortField.ConfigName => dto.SortDescending
                    ? query.OrderByDescending(h => h.ConfigName)
                    : query.OrderBy(h => h.ConfigName),
                HitSortField.Date => dto.SortDescending
                    ? query.OrderByDescending(h => h.Date)
                    : query.OrderBy(h => h.Date),
                HitSortField.WordlistName => dto.SortDescending
                    ? query.OrderByDescending(h => h.WordlistName)
                    : query.OrderBy(h => h.WordlistName),
                HitSortField.Proxy => dto.SortDescending
                    ? query.OrderByDescending(h => h.Proxy)
                    : query.OrderBy(h => h.Proxy),
                HitSortField.CapturedData => dto.SortDescending
                    ? query.OrderByDescending(h => h.CapturedData)
                    : query.OrderBy(h => h.CapturedData),
                _ => query.OrderByDescending(h => h.Date)
            };
        }
        else
        {
            query = query.OrderByDescending(h => h.Date);
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
