using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Exceptions;
using RuriLib.Models.Jobs;
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
    private readonly JobManagerService _jobManagerService;
    private readonly IMapper _mapper;
    private readonly ILogger<JobMonitorController> _logger;

    /// <summary></summary>
    public JobMonitorController(JobMonitorService jobMonitorService,
        JobManagerService jobManagerService,
        IMapper mapper, ILogger<JobMonitorController> logger)
    {
        _jobMonitorService = jobMonitorService;
        _jobManagerService = jobManagerService;
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
        return Ok(actions.Select(MapTriggeredAction));
    }

    /// <summary>
    /// Get a triggered action by id.
    /// </summary>
    [HttpGet("triggered-action")]
    [MapToApiVersion("1.0")]
    public ActionResult<TriggeredActionDto> Get(string id)
    {
        var actions = _jobMonitorService.TriggeredActions;
        var targetAction = actions.FirstOrDefault(a => a.Id == id);
        
        if (targetAction is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.TRIGGERED_ACTION_NOT_FOUND,
                id, nameof(IGuestRepository));
        }
        
        return MapTriggeredAction(targetAction);
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
        
        _logger.LogInformation("Updated triggered action {Id}", newAction.Id);

        return MapTriggeredAction(newAction);
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
        
        _logger.LogInformation("Created triggered action {Id}", newAction.Id);

        return MapTriggeredAction(newAction);
    }

    /// <summary>
    /// Resets a triggered action's execution counter.
    /// </summary>
    [HttpPost("triggered-action/reset")]
    public ActionResult Reset(string id)
    {
        var actions = _jobMonitorService.TriggeredActions;
        var targetAction = actions.FirstOrDefault(a => a.Id == id);

        if (targetAction is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.TRIGGERED_ACTION_NOT_FOUND,
                id, nameof(IGuestRepository));
        }

        targetAction.Reset();
        _jobMonitorService.SaveStateIfChanged();
        
        _logger.LogInformation("Reset triggered action {Id}", id);

        return Ok();
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
        
        _logger.LogInformation("Deleted triggered action {Id}", id);

        return Ok();
    }
    
    private TriggeredActionDto MapTriggeredAction(TriggeredAction action)
    {
        var mapped = _mapper.Map<TriggeredActionDto>(action);
        
        // Search for a job with the given id and set its name
        var job = _jobManagerService.Jobs.FirstOrDefault(j => j.Id == mapped.JobId);
        
        if (job is not null)
        {
            mapped.JobName = job.Name;
        }
        
        return mapped;
    }
}
