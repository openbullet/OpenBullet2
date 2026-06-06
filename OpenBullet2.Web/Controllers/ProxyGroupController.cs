using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.ProxyGroup;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Interfaces;
using OpenBullet2.Web.Models.Identity;
using RuriLib.Models.Jobs;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage proxy groups.
/// </summary>
/// <remarks></remarks>
[TypeFilter<GuestFilter>]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/proxy-group")]
public class ProxyGroupController(IProxyGroupRepository proxyGroupRepo,
    IProxyRepository proxyRepo, IGuestRepository guestRepo, IObjectMapper mapper,
    JobManagerService jobManagerService, ILogger<ProxyGroupController> logger) : ApiController
{
    private readonly IGuestRepository _guestRepo = guestRepo;
    private readonly JobManagerService _jobManagerService = jobManagerService;
    private readonly ILogger<ProxyGroupController> _logger = logger;
    private readonly IObjectMapper _mapper = mapper;
    private readonly IProxyGroupRepository _proxyGroupRepo = proxyGroupRepo;
    private readonly IProxyRepository _proxyRepo = proxyRepo;

    /// <summary>
    /// List all of the available proxy groups.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<ProxyGroupDto>>> GetAllGroups(CancellationToken cancellationToken)
    {
        var apiUser = HttpContext.GetApiUser();

        var entities = apiUser.Role is UserRole.Admin
            ? await _proxyGroupRepo.GetAll().Include(g => g.Owner).ToListAsync(cancellationToken)
            : await _proxyGroupRepo.GetAll().Include(g => g.Owner)
                .Where(g => g.Owner != null && g.Owner.Id == apiUser.Id).ToListAsync(cancellationToken);

        return Ok(_mapper.Map<IEnumerable<ProxyGroupDto>>(entities));
    }

    /// <summary>
    /// Create a new proxy group.
    /// </summary>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ProxyGroupDto>> Create(CreateProxyGroupDto dto,
        [FromServices] IValidator<CreateProxyGroupDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var apiUser = HttpContext.GetApiUser();

        var entity = _mapper.Map<ProxyGroupEntity>(dto);

        // If the user is a guest, we also have to mark them as the
        // owner of the proxy group, for access control purposes.
        if (apiUser.Role is UserRole.Guest)
        {
            var owner = await _guestRepo.GetAsync(apiUser.Id, cancellationToken);
            entity.Owner = owner;
        }

        await _proxyGroupRepo.AddAsync(entity, cancellationToken);

        _logger.LogInformation("Created a new proxy group with name {Name}", dto.Name);

        return _mapper.Map<ProxyGroupDto>(entity);
    }

    /// <summary>
    /// Update a proxy group.
    /// </summary>
    [HttpPut]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ProxyGroupDto>> Update(UpdateProxyGroupDto dto,
        [FromServices] IValidator<UpdateProxyGroupDto> validator,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(dto, cancellationToken);

        var entity = await GetEntityAsync(dto.Id, cancellationToken);

        EnsureOwnership(entity);

        _mapper.Map(dto, entity);
        await _proxyGroupRepo.UpdateAsync(entity, cancellationToken);

        _logger.LogInformation("Updated the information of the proxy group with id {Id}", entity.Id);

        return _mapper.Map<ProxyGroupDto>(entity);
    }

    /// <summary>
    /// Delete a proxy group. This will in turn delete all the proxies
    /// of this proxy group. Returns the number of proxies that were deleted.
    /// </summary>
    /// <param name="id">The id of the proxy group to delete</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await GetEntityAsync(id, cancellationToken);

        EnsureOwnership(entity);

        // Get the first proxy of the group
        var proxies = await _proxyRepo.GetAll()
            .Include(p => p.Group)
            .Where(p => p.Group != null && p.Group.Id == id)
            .ToListAsync(cancellationToken);

        var firstProxyId = proxies.FirstOrDefault()?.Id;

        foreach (var job in _jobManagerService.Jobs)
        {
            // If we find the first proxy in any of the jobs of type
            // ProxyCheckJob, throw
            if (job is ProxyCheckJob pcJob)
            {
                if (firstProxyId is not null && pcJob.Proxies?.Any(p => p.Id == firstProxyId) == true)
                {
                    throw new ResourceInUseException(
                        ErrorCode.ProxyGroupInUse,
                        $"The proxy group with id {id} is being used in job {job.Id} and cannot be deleted");
                }
            }
            // If we find a proxy source that is using this group in a
            // MultiRunJob, throw
            else if (job is MultiRunJob mrJob)
            {
                var sources = mrJob.ProxySources.OfType<GroupProxySource>();

                if (sources.Any(s => s.GroupId == id))
                {
                    throw new ResourceInUseException(
                        ErrorCode.ProxyGroupInUse,
                        $"The proxy group with id {id} is being used in job {job.Id} and cannot be deleted");
                }
            }
        }

        // This will cascade delete all the proxies in the group
        await _proxyGroupRepo.DeleteAsync(entity, cancellationToken);

        _logger.LogInformation("Deleted the proxy group with id {Id} and {ProxyCount} proxies",
            entity.Id, proxies.Count);

        return new AffectedEntriesDto { Count = proxies.Count };
    }

    private async Task<ProxyGroupEntity> GetEntityAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _proxyGroupRepo.GetAsync(id, cancellationToken) ?? throw new EntryNotFoundException(ErrorCode.ProxyGroupNotFound,
                id, nameof(IProxyGroupRepository));
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
