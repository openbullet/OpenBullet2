using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Dtos.JobMonitor;
using OpenBullet2.Web.Exceptions;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Monitor;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage the job monitor and triggered actions.
/// </summary>
[TypeFilter<AdminFilter>]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/job-monitor")]
public class JobMonitorController : ApiController
{
    private readonly JobManagerService _jobManagerService;
    private readonly JobMonitorService _jobMonitorService;
    private readonly ILogger<JobMonitorController> _logger;
    private readonly IMapper _mapper;

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
    public ActionResult<TriggeredActionDto> Get(string id) =>
        MapTriggeredAction(GetTriggeredAction(id));

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
    /// Update a triggered action.
    /// </summary>
    [HttpPut("triggered-action")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<TriggeredActionDto>> Update(
        UpdateTriggeredActionDto dto,
        [FromServices] IValidator<UpdateTriggeredActionDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        var targetAction = GetTriggeredAction(dto.Id);

        var newAction = _mapper.Map(dto, targetAction);
        _jobMonitorService.SaveStateIfChanged();

        _logger.LogInformation("Updated triggered action {Id}", newAction.Id);

        return MapTriggeredAction(newAction);
    }

    /// <summary>
    /// Resets a triggered action's execution counter.
    /// </summary>
    [HttpPost("triggered-action/reset")]
    [MapToApiVersion("1.0")]
    public ActionResult Reset(string id)
    {
        var targetAction = GetTriggeredAction(id);

        targetAction.Reset();
        _jobMonitorService.SaveStateIfChanged();

        _logger.LogInformation("Reset triggered action {Id}", id);

        return Ok();
    }

    /// <summary>
    /// Sets a triggered action as active or inactive.
    /// </summary>
    [HttpPost("triggered-action/set-active")]
    [MapToApiVersion("1.0")]
    public ActionResult SetActive(string id, bool active)
    {
        var targetAction = GetTriggeredAction(id);

        targetAction.IsActive = active;
        _jobMonitorService.SaveStateIfChanged();

        _logger.LogInformation("Set triggered action {Id} as {Active}", id, active ? "active" : "inactive");

        return Ok();
    }

    /// <summary>
    /// Delete a triggered action.
    /// </summary>
    [HttpDelete("triggered-action")]
    [MapToApiVersion("1.0")]
    public ActionResult Delete(string id)
    {
        var targetAction = GetTriggeredAction(id);

        _jobMonitorService.TriggeredActions.Remove(targetAction);
        _jobMonitorService.SaveStateIfChanged();

        _logger.LogInformation("Deleted triggered action {Id}", id);

        return Ok();
    }

    private TriggeredAction GetTriggeredAction(string id)
    {
        var actions = _jobMonitorService.TriggeredActions;
        var targetAction = actions.Find(a => a.Id == id);

        if (targetAction is null)
        {
            throw new EntryNotFoundException(
                ErrorCode.TriggeredActionNotFound,
                id, nameof(IGuestRepository));
        }

        return targetAction;
    }

    private TriggeredActionDto MapTriggeredAction(TriggeredAction action)
    {
        var mapped = _mapper.Map<TriggeredActionDto>(action);

        // Search for a job with the given id and set its name
        var job = _jobManagerService.Jobs.FirstOrDefault(j => j.Id == mapped.JobId);

        if (job is not null)
        {
            mapped.JobName = job.Name;
            mapped.JobType = GetJobType(job);
        }

        return mapped;
    }

    private static JobType GetJobType(Job job) =>
        job switch {
            MultiRunJob => JobType.MultiRun,
            ProxyCheckJob => JobType.ProxyCheck,
            _ => throw new NotImplementedException()
        };
}
