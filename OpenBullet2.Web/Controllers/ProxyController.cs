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
    /// the given id (all by default), supports pagination.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<PagedList<ProxyDto>>> GetAll(
        [FromQuery] PaginationDto pagination, int proxyGroupId = -1)
    {
        var apiUser = HttpContext.GetApiUser();

        var query = apiUser.Role is UserRole.Admin
            ? _proxyRepo.GetAll()
            : _proxyRepo.GetAll().Include(p => p.Group)
                .ThenInclude(g => g.Owner)
                .Where(p => p.Group.Owner.Id == apiUser.Id);

        if (proxyGroupId != -1)
        {
            query = query.Where(p => p.Group.Id == proxyGroupId);
        }

        var pagedEntities = await PagedList<ProxyEntity>.CreateAsync(query,
            pagination.PageNumber, pagination.PageSize);

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

        await _proxyRepo.Add(entities);

        var duplicatesCount = await _proxyRepo.RemoveDuplicates(dto.ProxyGroupId);

        _logger.LogInformation($"Added {entities.Count - duplicatesCount} unique new proxies to proxy group {groupEntity.Name}");

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
                new string[] { "\r\n", "\n" },
                StringSplitOptions.RemoveEmptyEntries);
        }
        catch (Exception ex)
        {
            throw new RemoteFetchFailedException(
                ErrorCode.REMOTE_RESOURCE_FETCH_FAILED,
                $"Failed to fetch proxies from {dto.Url}. Reason: {ex.Message}");
        }

        var entities = ParseProxies(lines, dto.DefaultType,
            dto.DefaultUsername, dto.DefaultPassword).ToList();

        entities.ForEach(e => e.Group = groupEntity);

        await _proxyRepo.Add(entities);

        var duplicatesCount = await _proxyRepo.RemoveDuplicates(dto.ProxyGroupId);

        _logger.LogInformation($"Added {entities.Count - duplicatesCount} unique new proxies to proxy group {groupEntity.Name}");

        return new AffectedEntriesDto
        {
            Count = entities.Count - duplicatesCount
        };
    }

    /// <summary>
    /// Delete all the proxies of a group. Returns the number of deleted proxies.
    /// </summary>
    [HttpDelete("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteAllProxies(
        int proxyGroupId)
    {
        var groupEntity = await GetProxyGroupEntityAsync(proxyGroupId);
        EnsureOwnership(groupEntity);

        var toDelete = await _proxyRepo.GetAll()
            .Include(p => p.Group)
            .Where(p => p.Group.Id == proxyGroupId)
            .ToListAsync();

        await _proxyRepo.Delete(toDelete);

        _logger.LogInformation($"Deleted {toDelete.Count} proxies from proxy group {groupEntity.Name}");

        return new AffectedEntriesDto
        {
            Count = toDelete.Count
        };
    }

    /// <summary>
    /// Delete all proxies with a given status in a group.
    /// Returns the number of deleted proxies.
    /// </summary>
    [HttpDelete("by-status")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteByStatus(
        int proxyGroupId, ProxyWorkingStatus status)
    {
        var groupEntity = await GetProxyGroupEntityAsync(proxyGroupId);
        EnsureOwnership(groupEntity);

        var toDelete = await _proxyRepo.GetAll()
            .Include(p => p.Group)
            .Where(p => p.Status == status)
            .Where(p => p.Group.Id == proxyGroupId)
            .ToListAsync();

        await _proxyRepo.Delete(toDelete);

        _logger.LogInformation($"Deleted {toDelete.Count} proxies from proxy group {groupEntity.Name}");

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

        await _proxyRepo.Delete(toDelete);

        _logger.LogInformation($"Deleted {toDelete.Count} proxies from proxy group {groupEntity.Name}");

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
                _logger.LogWarning($"Failed to parse proxy {line}");
            }
        }

        return proxies.Select(p =>
            Core.Helpers.Mapper.MapProxyToProxyEntity(p));
    }

    private async Task<ProxyGroupEntity> GetProxyGroupEntityAsync(int id)
    {
        var entity = await _proxyGroupRepo.Get(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.PROXY_GROUP_NOT_FOUND,
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
            _logger.LogWarning($"Guest user {apiUser.Username} tried to access a proxy group not owned by them");

            throw new EntryNotFoundException(ErrorCode.PROXY_GROUP_NOT_FOUND,
                entity.Id, nameof(IProxyGroupRepository));
        }
    }
}
