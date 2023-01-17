using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Proxy;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using OpenBullet2.Web.Models.Pagination;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage proxies.
/// </summary>
[Guest]
[ApiVersion("1.0")]
public class ProxyController : ApiController
{
    private readonly IProxyRepository _proxyRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<ProxyController> _logger;

    /// <summary></summary>
    public ProxyController(IProxyRepository proxyRepo, IMapper mapper,
        ILogger<ProxyController> logger)
    {
        _proxyRepo = proxyRepo;
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
}
