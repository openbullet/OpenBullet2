using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Exceptions;
using RuriLib.Models.Jobs.Monitor;

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
    public ActionResult<IEnumerable<TriggeredActionDto>> GetAll()
    {
        var actions = _jobMonitorService.TriggeredActions;
        return Ok(_mapper.Map<IEnumerable<TriggeredActionDto>>(actions));
    }

    /// <summary>
    /// Update a triggered action.
    /// </summary>
    [HttpPut("triggered-action")]
    [MapToApiVersion("1.0")]
    public ActionResult<TriggeredActionDto> Update(
        UpdateTriggeredActionDto dto)
    {
        var actions = _jobMonitorService.TriggeredActions;
        var targetAction = actions.FirstOrDefault(a => a.Id == dto.Id);
        
        if (targetAction is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.TRIGGERED_ACTION_NOT_FOUND,
                dto.Id, nameof(IGuestRepository));
        }

        var newAction = _mapper.Map(dto, targetAction);
        _jobMonitorService.SaveStateIfChanged();

        return _mapper.Map<TriggeredActionDto>(newAction);
    }

    /// <summary>
    /// Create a new triggered action.
    /// </summary>
    [HttpPost("triggered-action")]
    [MapToApiVersion("1.0")]
    public ActionResult<TriggeredActionDto> Create(
        CreateTriggeredActionDto dto)
    {
        var actions = _jobMonitorService.TriggeredActions;

        var newAction = _mapper.Map<TriggeredAction>(dto);
        actions.Add(newAction);
        _jobMonitorService.SaveStateIfChanged();

        return _mapper.Map<TriggeredActionDto>(newAction);
    }

    /// <summary>
    /// Delete a triggered action.
    /// </summary>
    [HttpDelete("triggered-action")]
    [MapToApiVersion("1.0")]
    public ActionResult Delete(string id)
    {
        var actions = _jobMonitorService.TriggeredActions;
        var targetAction = actions.FirstOrDefault(a => a.Id == id);

        if (targetAction is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.TRIGGERED_ACTION_NOT_FOUND,
                id, nameof(IGuestRepository));
        }

        actions.Remove(targetAction);
        _jobMonitorService.SaveStateIfChanged();

        return Ok();
    }
}
