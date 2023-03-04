using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Attributes;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using RuriLib.Models.Jobs;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage jobs.
/// </summary>
[Guest]
[ApiVersion("1.0")]
public class JobController : ApiController
{
    private readonly IJobRepository _jobRepo;
    private readonly ILogger<JobController> _logger;
    private readonly IGuestRepository _guestRepo;
    private readonly IMapper _mapper;
    private readonly JobManagerService _jobManager;
    private readonly JobFactoryService _jobFactory;

    /// <summary></summary>
    public JobController(IJobRepository jobRepo, ILogger<JobController> logger,
        IGuestRepository guestRepo, IMapper mapper, JobManagerService jobManager,
        JobFactoryService jobFactory)
    {
        _jobRepo = jobRepo;
        _logger = logger;
        _guestRepo = guestRepo;
        _mapper = mapper;
        _jobManager = jobManager;
        _jobFactory = jobFactory;
    }

    /// <summary>
    /// Get overview information about all multi run jobs.
    /// </summary>
    [HttpGet("multi-run/all")]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<MultiRunJobOverviewDto>>
        GetAllMultiRunJobs()
    {
        var apiUser = HttpContext.GetApiUser();

        // Only get the jobs of the user!
        var jobs = _jobManager.Jobs
            .Where(j => CanSee(apiUser, j) && j is MultiRunJob)
            .Cast<MultiRunJob>();

        return Ok(_mapper.Map<IEnumerable<MultiRunJobOverviewDto>>(jobs));
    }

    /// <summary>
    /// Get overview information about all proxy check jobs.
    /// </summary>
    [HttpGet("proxy-check/all")]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<ProxyCheckJobOverviewDto>>
        GetAllProxyCheckJobs()
    {
        var apiUser = HttpContext.GetApiUser();

        // Only get the jobs of the user!
        var jobs = _jobManager.Jobs
            .Where(j => CanSee(apiUser, j) && j is ProxyCheckJob)
            .Cast<ProxyCheckJob>();

        return Ok(_mapper.Map<IEnumerable<ProxyCheckJobOverviewDto>>(jobs));
    }

    /// <summary>
    /// Get the details of a multi run job.
    /// </summary>
    [HttpGet("multi-run")]
    [MapToApiVersion("1.0")]
    public ActionResult<MultiRunJobDto> GetMultiRunJob(int id)
    {
        var job = GetJob(id);
        EnsureOwnership(job);

        if (job is not MultiRunJob mrJob)
        {
            throw new BadRequestException(
                ErrorCode.INVALID_JOB_TYPE,
                $"The job with id {id} is not a multi run job");
        }

        return _mapper.Map<MultiRunJobDto>(mrJob);
    }

    /// <summary>
    /// Get the details of a proxy check job.
    /// </summary>
    [HttpGet("proxy-check")]
    [MapToApiVersion("1.0")]
    public ActionResult<ProxyCheckJobDto> GetProxyCheckJob(int id)
    {
        var job = GetJob(id);
        EnsureOwnership(job);

        if (job is not ProxyCheckJob pcJob)
        {
            throw new BadRequestException(
                ErrorCode.INVALID_JOB_TYPE,
                $"The job with id {id} is not a proxy check job");
        }

        return _mapper.Map<ProxyCheckJobDto>(pcJob);
    }

    /// <summary>
    /// Get the options of a multi run job. If <paramref name="id"/> is -1,
    /// the default options will be provided.
    /// </summary>
    [HttpGet("multi-run/options")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<MultiRunJobOptionsDto>> GetMultiRunJobOptions(
        int id = -1)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get the options of a proxy check job. If <paramref name="id"/> is -1,
    /// the default options will be provided.
    /// </summary>
    [HttpGet("proxy-check/options")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ProxyCheckJobOptionsDto>> GetProxyCheckJobOptions(
        int id = -1)
    {
        if (id == -1)
        {
            var options = JobOptionsFactory.CreateNew(JobType.ProxyCheck);
            return _mapper.Map<ProxyCheckJobOptionsDto>(options);
        }

        var entity = await GetEntityAsync(id);
        EnsureOwnership(entity);

        var jsonSettings = new JsonSerializerSettings 
        { 
            TypeNameHandling = TypeNameHandling.Auto
        };

        var jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(
            entity.JobOptions, jsonSettings)?.Options;

        if (jobOptions is null)
        {
            throw new Exception("The job options are null");
        }

        if (jobOptions is not ProxyCheckJobOptions pcJobOptions)
        {
            throw new Exception("Invalid job options type");
        }

        return _mapper.Map<ProxyCheckJobOptionsDto>(pcJobOptions);
    }

    /// <summary>
    /// Create a multi run job.
    /// </summary>
    [HttpPost("multi-run")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<MultiRunJobDto>> CreateMultiRunJob(
        CreateMultiRunJobDto dto)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Create a proxy check job.
    /// </summary>
    [HttpPost("proxy-check")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ProxyCheckJobDto>> CreateProxyCheckJob(
        CreateProxyCheckJobDto dto)
    {
        var apiUser = HttpContext.GetApiUser();
        var jobOptions = _mapper.Map<ProxyCheckJobOptions>(dto);

        var jsonSettings = new JsonSerializerSettings 
        { 
            TypeNameHandling = TypeNameHandling.Auto 
        };
        
        var wrapper = new JobOptionsWrapper 
        { 
            Options = jobOptions
        };

        var entity = new JobEntity
        {
            Owner = await _guestRepo.Get(apiUser.Id),
            CreationDate = DateTime.Now,
            JobType = JobType.ProxyCheck,
            JobOptions = JsonConvert.SerializeObject(wrapper, jsonSettings)
        };

        await _jobRepo.Add(entity);

        // This might fail and we would have inconsistencies!
        // If that happens, remove the entity and rethrow
        try
        {
            var job = _jobFactory.FromOptions(entity.Id, apiUser.Id, jobOptions);
            _jobManager.AddJob(job);

            return _mapper.Map<ProxyCheckJobDto>((ProxyCheckJob)job);
        }
        catch
        {
            await _jobRepo.Delete(entity);
            throw;
        }
    }

    /// <summary>
    /// Update a multi run job.
    /// </summary>
    [HttpPut("multi-run")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<MultiRunJobDto>> UpdateMultiRunJob(
        UpdateMultiRunJobDto dto)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Update a proxy check job.
    /// </summary>
    [HttpPut("proxy-check")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ProxyCheckJobDto>> UpdateProxyCheckJob(
        UpdateProxyCheckJobDto dto)
    {
        // Make sure job is idle
        var job = GetJob(dto.Id);
        EnsureOwnership(job);

        if (job.Status is not JobStatus.Idle)
        {
            throw new ResourceInUseException(ErrorCode.JOB_NOT_IDLE,
                $"Job {dto.Id} is not idle");
        }

        if (job is not ProxyCheckJob pcJob)
        {
            throw new BadRequestException(
                ErrorCode.INVALID_JOB_TYPE,
                $"The job with id {dto.Id} is not a proxy check job");
        }

        var entity = await GetEntityAsync(dto.Id);
        EnsureOwnership(entity);

        var jobOptions = _mapper.Map<ProxyCheckJobOptions>(dto);

        var jsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        var wrapper = new JobOptionsWrapper { Options = jobOptions };
        entity.JobOptions = JsonConvert.SerializeObject(wrapper, jsonSettings);

        await _jobRepo.Update(entity);

        var oldJob = _jobManager.Jobs.First(j => j.Id == dto.Id);

        var newJob = _jobFactory.FromOptions(
            dto.Id, entity.Owner == null ? 0 : entity.Owner.Id, jobOptions);

        _jobManager.RemoveJob(oldJob);
        _jobManager.AddJob(newJob);

        return _mapper.Map<ProxyCheckJobDto>((ProxyCheckJob)newJob);
    }

    /// <summary>
    /// Set the number of bots.
    /// </summary>
    [HttpPatch("bot-number")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> SetBotNumber(SetBotNumberDto dto)
    {
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        switch (job)
        {
            case MultiRunJob mrj:
                await mrj.ChangeBots(dto.BotNumber);
                break;

            case ProxyCheckJob pcj:
                await pcj.ChangeBots(dto.BotNumber);
                break;

            default:
                throw new NotSupportedException();
        }

        return Ok();
    }

    /// <summary>
    /// Delete a job.
    /// </summary>
    [HttpDelete]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> Delete(int id)
    {
        // Double check just to be sure
        var entity = await GetEntityAsync(id);
        var job = GetJob(id);

        EnsureOwnership(entity);
        EnsureOwnership(job);

        await _jobRepo.Delete(entity);
        _jobManager.RemoveJob(job);

        return Ok();
    }

    /// <summary>
    /// Delete all jobs.
    /// </summary>
    [HttpDelete("all")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<AffectedEntriesDto>> DeleteAll()
    {
        var apiUser = HttpContext.GetApiUser();
        var deletedCount = 0;

        // If any job not idle, throw!
        var notIdleJobs = _jobManager.Jobs
            .Where(j => CanSee(apiUser, j) && j.Status != JobStatus.Idle);

        if (notIdleJobs.Any())
        {
            throw new ResourceInUseException(ErrorCode.JOB_NOT_IDLE,
                "There are non-idle jobs, please stop them first");
        }

        // If admin, just purge all
        if (apiUser.Role is UserRole.Admin)
        {
            deletedCount = await _jobRepo.GetAll().CountAsync();

            _jobRepo.Purge();
            _jobManager.Clear();
        }
        else
        {
            var entities = await _jobRepo.GetAll()
                .Include(j => j.Owner)
                .Where(j => j.Owner.Id == apiUser.Id)
                .ToListAsync();

            deletedCount = entities.Count;

            await _jobRepo.Delete(entities);

            foreach (var job in _jobManager.Jobs.Where(j => j.OwnerId == apiUser.Id))
            {
                _jobManager.RemoveJob(job);
            }
        }

        return new AffectedEntriesDto
        {
            Count = deletedCount
        };
    }
    
    private Job GetJob(int id)
    {
        var job = _jobManager.Jobs.FirstOrDefault(j => j.Id == id);

        if (job is null)
        {
            throw new EntryNotFoundException(ErrorCode.JOB_NOT_FOUND,
                id, nameof(JobManagerService));
        }

        return job;
    }

    private async Task<JobEntity> GetEntityAsync(int id)
    {
        var entity = await _jobRepo.Get(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.JOB_NOT_FOUND,
                id, nameof(IJobRepository));
        }

        return entity;
    }

    private bool CanSee(ApiUser apiUser, Job job)
        => apiUser.Role is UserRole.Admin || job.Id == apiUser.Id;

    private void EnsureOwnership(Job job)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is trying to access a job that is not owned
        // by them, throw a not found exception.
        if (apiUser.Role is UserRole.Guest && apiUser.Id != job.OwnerId)
        {
            _logger.LogWarning($"Guest user {apiUser.Username} tried to access a job not owned by them");

            throw new EntryNotFoundException(ErrorCode.JOB_NOT_FOUND,
                job.Id, nameof(JobManagerService));
        }
    }

    private void EnsureOwnership(JobEntity entity)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is trying to access a job that is not owned
        // by them, throw a not found exception.
        if (apiUser.Role is UserRole.Guest && apiUser.Id != entity.Owner?.Id)
        {
            _logger.LogWarning($"Guest user {apiUser.Username} tried to access a job not owned by them");

            throw new EntryNotFoundException(ErrorCode.JOB_NOT_FOUND,
                entity.Id, nameof(IJobRepository));
        }
    }
}
