using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Proxy;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using OpenBullet2.Web.Models.Pagination;
using RuriLib.Models.Proxies;
using System.Text;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage proxies.
/// </summary>
[Guest]
[ApiVersion("1.0")]
public class ProxyController : ApiController
{
    private readonly IProxyRepository _proxyRepo;
    private readonly IProxyGroupRepository _proxyGroupRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<ProxyController> _logger;

    /// <summary></summary>
    public ProxyController(IProxyRepository proxyRepo,
        IProxyGroupRepository proxyGroupRepo, IMapper mapper,
        ILogger<ProxyController> logger)
    {
        _proxyRepo = proxyRepo;
        _proxyGroupRepo = proxyGroupRepo;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// List all of the available proxies for the proxy group with
    /// the given id (all by default), supports filtering and pagination.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PagedList<ProxyDto>>> GetAll(
        [FromQuery] ProxyFiltersDto dto)
    {
        var query = FilteredQuery(dto);

        var pagedEntities = await PagedList<ProxyEntity>.CreateAsync(query,
            dto.PageNumber, dto.PageSize);

        return _mapper.Map<PagedList<ProxyDto>>(pagedEntities);
    }

    /// <summary>
    /// Add proxies to a proxy group. Discards duplicates.
    /// Returns the number of unique new proxies added.
    /// </summary>
    [HttpPost("add")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> Add(
        AddProxiesFromListDto dto)
    {
        var groupEntity = await GetProxyGroupEntityAsync(dto.ProxyGroupId);
        EnsureOwnership(groupEntity);

        var entities = ParseProxies(dto.Proxies, dto.DefaultType,
            dto.DefaultUsername, dto.DefaultPassword).ToList();

        entities.ForEach(e => e.Group = groupEntity);

        await _proxyRepo.AddAsync(entities);

        var duplicatesCount = await _proxyRepo.RemoveDuplicatesAsync(dto.ProxyGroupId);

        _logger.LogInformation("Added {ProxyCount} unique new proxies to proxy group {Name}",
            entities.Count - duplicatesCount, groupEntity.Name);

        return new AffectedEntriesDto
        {
            Count = entities.Count - duplicatesCount
        };
    }

    /// <summary>
    /// Add proxies to a proxy group by fetching them from a remote source.
    /// Discards duplicates. Returns the number of unique new proxies added.
    /// </summary>
    [HttpPost("add-from-remote")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> AddFromRemote(
        AddProxiesFromRemoteDto dto)
    {
        var groupEntity = await GetProxyGroupEntityAsync(dto.ProxyGroupId);
        EnsureOwnership(groupEntity);

        string[] lines;

        try
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "User-Agent", Globals.UserAgent);

            using var response = await client.GetAsync(dto.Url);
            var text = await response.Content.ReadAsStringAsync();

            lines = text.Split(
                ["\r\n", "\n"],
                StringSplitOptions.RemoveEmptyEntries);
        }
        catch (Exception ex)
        {
            throw new RemoteFetchFailedException(
                ErrorCode.RemoteResourceFetchFailed,
                $"Failed to fetch proxies from {dto.Url}. Reason: {ex.Message}");
        }

        var entities = ParseProxies(lines, dto.DefaultType,
            dto.DefaultUsername, dto.DefaultPassword).ToList();

        entities.ForEach(e => e.Group = groupEntity);

        await _proxyRepo.AddAsync(entities);

        var duplicatesCount = await _proxyRepo.RemoveDuplicatesAsync(dto.ProxyGroupId);

        _logger.LogInformation("Added {ProxyCount} unique new proxies to proxy group {Name}",
            entities.Count - duplicatesCount, groupEntity.Name);

        return new AffectedEntriesDto
        {
            Count = entities.Count - duplicatesCount
        };
    }

    /// <summary>
    /// Move all proxies that match the filters from one group to another.
    /// Returns the number of moved proxies.
    /// </summary>
    [HttpPost("move/many")] 
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> MoveMany(
        MoveProxiesDto dto)
    {
        var destinationGroupEntity = await GetProxyGroupEntityAsync(
            dto.DestinationGroupId);
        EnsureOwnership(destinationGroupEntity);

        var query = FilteredQuery(dto);

        var toMove = await query.ToListAsync();
        toMove.ForEach(p => p.Group = destinationGroupEntity);

        await _proxyRepo.UpdateAsync(toMove);
        await _proxyRepo.RemoveDuplicatesAsync(dto.DestinationGroupId);

        _logger.LogInformation("Moved {ProxyCount} proxies", toMove.Count);

        return new AffectedEntriesDto
        {
            Count = toMove.Count
        };
    }

    /// <summary>
    /// Download a txt file with one proxy per line, containing all proxies
    /// that match the filters. Proxies will be formatted like
    /// (type)host:port:username:password
    /// </summary>
    [HttpGet("download/many")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DownloadMany([FromQuery] ProxyFiltersDto dto)
    {
        var query = FilteredQuery(dto);
        var proxies = await query.ToListAsync();
        var outputProxies = string.Join(Environment.NewLine, proxies);
        var bytes = Encoding.UTF8.GetBytes(outputProxies);
        return File(bytes, "text/plain", "proxies.txt");
    }

    /// <summary>
    /// Delete all proxies that match the filters in a group.
    /// Returns the number of deleted proxies.
    /// </summary>
    [HttpDelete("many")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteMany(
        [FromQuery] ProxyFiltersDto dto)
    {
        var query = FilteredQuery(dto);

        var toDelete = await query.ToListAsync();

        await _proxyRepo.DeleteAsync(toDelete);

        _logger.LogInformation("Deleted {ProxyCount} proxies", toDelete.Count);

        return new AffectedEntriesDto
        {
            Count = toDelete.Count
        };
    }

    /// <summary>
    /// Delete all working proxies with ping above a given threshold in a group.
    /// Returns the number of deleted proxies.
    /// </summary>
    [HttpDelete("slow")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteSlow(
        int proxyGroupId, int maxPing = 10000)
    {
        var groupEntity = await GetProxyGroupEntityAsync(proxyGroupId);
        EnsureOwnership(groupEntity);

        var toDelete = await _proxyRepo.GetAll()
            .Include(p => p.Group)
            .Where(p => p.Status == ProxyWorkingStatus.Working)
            .Where(p => p.Ping > maxPing)
            .Where(p => p.Group.Id == proxyGroupId)
            .ToListAsync();

        await _proxyRepo.DeleteAsync(toDelete);

        _logger.LogInformation("Deleted {HitCount} proxies from proxy group {Name}",
            toDelete.Count, groupEntity.Name);

        return new AffectedEntriesDto
        {
            Count = toDelete.Count
        };
    }

    private IEnumerable<ProxyEntity> ParseProxies(IEnumerable<string> lines,
        ProxyType defaultType, string defaultUsername, string defaultPassword)
    {
        var proxies = new List<Proxy>();

        foreach (var line in lines)
        {
            if (Proxy.TryParse(line, out var proxy, defaultType,
                defaultUsername, defaultPassword))
            {
                proxies.Add(proxy);
            }
            else
            {
                _logger.LogWarning("Failed to parse proxy {Line}", line);
            }
        }

        return proxies.Select(Core.Helpers.Mapper.MapProxyToProxyEntity);
    }

    private IQueryable<ProxyEntity> FilteredQuery(ProxyFiltersDto dto)
    {
        var apiUser = HttpContext.GetApiUser();

        var query = apiUser.Role is UserRole.Admin
            ? _proxyRepo.GetAll()
            : _proxyRepo.GetAll()
                .Include(p => p.Group)
                .ThenInclude(g => g.Owner)
                .Where(p => p.Group.Owner.Id == apiUser.Id);

        if (!string.IsNullOrEmpty(dto.SearchTerm))
        {
            query = query.Where(p =>
                EF.Functions.Like(p.Host, $"%{dto.SearchTerm}%") ||
                EF.Functions.Like(p.Port.ToString(), $"%{dto.SearchTerm}%") ||
                EF.Functions.Like(p.Username, $"%{dto.SearchTerm}%") ||
                EF.Functions.Like(p.Country, $"%{dto.SearchTerm}%"));
        }

        if (dto.Type is not null)
        {
            query = query.Where(p => p.Type == dto.Type);
        }

        if (dto.Status is not null)
        {
            query = query.Where(p => p.Status == dto.Status);
        }

        if (dto.ProxyGroupId != -1)
        {
            query = query.Where(p => p.Group.Id == dto.ProxyGroupId);
        }

        return query;
    }

    private async Task<ProxyGroupEntity> GetProxyGroupEntityAsync(int id)
    {
        var entity = await _proxyGroupRepo.GetAsync(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.ProxyGroupNotFound,
                id, nameof(IProxyGroupRepository));
        }

        return entity;
    }

    private void EnsureOwnership(ProxyGroupEntity entity)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is trying to access a proxy group that is not owned
        // by them, throw a not found exception.
        if (apiUser.Role is UserRole.Guest && apiUser.Id != entity.Owner?.Id)
        {
            _logger.LogWarning("Guest user {Username} tried to access a proxy group not owned by them",
                apiUser.Username);

            throw new EntryNotFoundException(ErrorCode.ProxyGroupNotFound,
                entity.Id, nameof(IProxyGroupRepository));
        }
    }
}
