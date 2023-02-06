using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.JobMonitor;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage the job monitor and triggered actions.
/// </summary>
[Admin]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/job-monitor")]
public class JobMonitorController : ApiController
{
    private readonly JobMonitorService _jobMonitorService;
    private readonly IMapper _mapper;
    private readonly ILogger<JobMonitorController> _logger;

    /// <summary></summary>
    public JobMonitorController(JobMonitorService jobMonitorService,
        IMapper mapper, ILogger<JobMonitorController> logger)
    {
        _jobMonitorService = jobMonitorService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// List all available triggered actions.
    /// </summary>
    [HttpGet("triggered-action/all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<IEnumerable<TriggeredActionDto>>> GetAll()
    {
        var actions = _jobMonitorService.TriggeredActions;
        return Ok(_mapper.Map<IEnumerable<TriggeredActionDto>>(actions));
    }
}
