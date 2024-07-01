using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Models.Hits;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Models.Proxies;
using OpenBullet2.Core.Models.Proxies.Sources;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Core.Services;
using OpenBullet2.Web.Auth;
using OpenBullet2.Web.Dtos.Common;
using OpenBullet2.Web.Dtos.Job;
using OpenBullet2.Web.Dtos.Job.MultiRun;
using OpenBullet2.Web.Dtos.Job.ProxyCheck;
using OpenBullet2.Web.Exceptions;
using OpenBullet2.Web.Extensions;
using OpenBullet2.Web.Models.Identity;
using OpenBullet2.Web.Utils;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Hits.HitOutputs;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.StartConditions;
using RuriLib.Models.Proxies.ProxySources;

namespace OpenBullet2.Web.Controllers;

/// <summary>
/// Manage jobs.
/// </summary>
[TypeFilter<GuestFilter>]
[ApiVersion("1.0")]
public class JobController : ApiController
{
    private readonly IGuestRepository _guestRepo;
    private readonly JobFactoryService _jobFactory;
    private readonly JobManagerService _jobManager;
    private readonly IJobRepository _jobRepo;
    private readonly ILogger<JobController> _logger;
    private readonly IMapper _mapper;
    private readonly IProxyGroupRepository _proxyGroupRepo;
    private readonly IRecordRepository _recordRepo;

    /// <summary></summary>
    public JobController(IJobRepository jobRepo, ILogger<JobController> logger,
        IGuestRepository guestRepo, IMapper mapper, JobManagerService jobManager,
        JobFactoryService jobFactory, IProxyGroupRepository proxyGroupRepo,
        IRecordRepository recordRepo)
    {
        _jobRepo = jobRepo;
        _logger = logger;
        _guestRepo = guestRepo;
        _mapper = mapper;
        _jobManager = jobManager;
        _jobFactory = jobFactory;
        _proxyGroupRepo = proxyGroupRepo;
        _recordRepo = recordRepo;
    }

    /// <summary>
    /// Get overview information about all jobs.
    /// </summary>
    [HttpGet("all")]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<JobOverviewDto>> GetAll()
    {
        var apiUser = HttpContext.GetApiUser();

        // Only get the jobs of the user!
        var jobs = _jobManager.Jobs
            .Where(j => CanSee(apiUser, j))
            .OrderBy(j => j.Id);

        var mapped = jobs.Select(job => new JobOverviewDto {
                Id = job.Id,
                OwnerId = job.OwnerId,
                Type = GetJobType(job),
                Status = job.Status,
                Name = job.Name
            })
            .ToList();

        return Ok(mapped);
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
            .Cast<MultiRunJob>()
            .OrderBy(j => j.Id);

        var dtos = new List<MultiRunJobOverviewDto>();

        foreach (var job in jobs)
        {
            var dataPoolInfo = job.DataPool switch {
                WordlistDataPool w => $"{w.Wordlist?.Name} (Wordlist)",
                CombinationsDataPool => "Combinations",
                InfiniteDataPool => "Infinite",
                RangeDataPool => "Range",
                FileDataPool f => $"{f.FileName} (File)",
                _ => throw new NotImplementedException()
            };

            var dto = new MultiRunJobOverviewDto {
                Id = job.Id,
                OwnerId = job.OwnerId,
                Type = JobType.MultiRun,
                Status = job.Status,
                Name = job.Name,
                ConfigName = job.Config?.Metadata.Name,
                UseProxies = job.ShouldUseProxies(),
                Bots = job.Bots,
                DataPoolInfo = dataPoolInfo,
                DataHits = job.DataHits,
                DataCustom = job.DataCustom,
                DataToCheck = job.DataToCheck,
                DataTotal = job.DataPool.Size,
                DataTested = job.Status is JobStatus.Idle ? job.Skip : job.DataTested + job.Skip,
                CPM = job.CPM,
                Progress = job.Progress < 0 ? 0 : job.Progress
            };

            dtos.Add(dto);
        }

        return Ok(dtos);
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
            .Cast<ProxyCheckJob>()
            .OrderBy(j => j.Id);

        var dtos = _mapper.Map<IEnumerable<ProxyCheckJobOverviewDto>>(jobs).ToList();

        dtos.ForEach(dto => dto.Type = JobType.ProxyCheck);
        return Ok(dtos);
    }

    /// <summary>
    /// Get the details of a multi run job.
    /// </summary>
    [HttpGet("multi-run")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<MultiRunJobDto>> GetMultiRunJob(int id)
    {
        var job = GetJob<MultiRunJob>(id);
        return await MapMultiRunJobDto(job);
    }

    /// <summary>
    /// Get the details of a proxy check job.
    /// </summary>
    [HttpGet("proxy-check")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ProxyCheckJobDto>> GetProxyCheckJob(int id)
    {
        var job = GetJob<ProxyCheckJob>(id);
        return await MapProxyCheckJobDto(job);
    }

    /// <summary>
    /// Get the options of a multi run job. If <paramref name="id" /> is -1,
    /// the default options will be provided.
    /// </summary>
    [HttpGet("multi-run/options")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<MultiRunJobOptionsDto>> GetMultiRunJobOptions(
        int id = -1)
    {
        if (id == -1)
        {
            var options = JobOptionsFactory.CreateNew(JobType.MultiRun);
            var mapped = _mapper.Map<MultiRunJobOptionsDto>(options);
            mapped.Name = $"{HttpContext.GetApiUser().Username}'s job";
            return mapped;
        }

        var entity = await GetEntityAsync(id);
        EnsureOwnership(entity);

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(
            entity.JobOptions, jsonSettings)?.Options;

        if (jobOptions is null)
        {
            throw new ApiException(ErrorCode.InvalidJobConfiguration, "The job options are null");
        }

        if (jobOptions is not MultiRunJobOptions mrjJobOptions)
        {
            throw new ApiException(ErrorCode.InvalidJobType, "Invalid job options type");
        }

        return _mapper.Map<MultiRunJobOptionsDto>(mrjJobOptions);
    }

    /// <summary>
    /// Get the options of a proxy check job. If <paramref name="id" /> is -1,
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
            var mapped = _mapper.Map<ProxyCheckJobOptionsDto>(options);
            mapped.Name = $"{HttpContext.GetApiUser().Username}'s job";
            return mapped;
        }

        var entity = await GetEntityAsync(id);
        EnsureOwnership(entity);

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(
            entity.JobOptions, jsonSettings)?.Options;

        if (jobOptions is null)
        {
            throw new ApiException(ErrorCode.InvalidJobConfiguration, "The job options are null");
        }

        if (jobOptions is not ProxyCheckJobOptions pcJobOptions)
        {
            throw new ApiException(ErrorCode.InvalidJobType, "Invalid job options type");
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
        var apiUser = HttpContext.GetApiUser();
        var jobOptions = _mapper.Map<MultiRunJobOptions>(dto);

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var wrapper = new JobOptionsWrapper { Options = jobOptions };

        var entity = new JobEntity {
            Owner = await _guestRepo.GetAsync(apiUser.Id),
            CreationDate = DateTime.UtcNow,
            JobType = JobType.MultiRun,
            JobOptions = JsonConvert.SerializeObject(wrapper, jsonSettings)
        };

        await _jobRepo.AddAsync(entity);
        
        _logger.LogInformation("Created a new multi run job with id {Id}", entity.Id);

        // This might fail and we would have inconsistencies!
        // If that happens, remove the entity and rethrow
        try
        {
            var job = _jobFactory.FromOptions(entity.Id, apiUser.Id, jobOptions);
            _jobManager.AddJob(job);

            return await MapMultiRunJobDto((MultiRunJob)job);
        }
        catch
        {
            await _jobRepo.DeleteAsync(entity);
            throw;
        }
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

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var wrapper = new JobOptionsWrapper { Options = jobOptions };

        var entity = new JobEntity {
            Owner = await _guestRepo.GetAsync(apiUser.Id),
            CreationDate = DateTime.UtcNow,
            JobType = JobType.ProxyCheck,
            JobOptions = JsonConvert.SerializeObject(wrapper, jsonSettings)
        };

        await _jobRepo.AddAsync(entity);
        
        _logger.LogInformation("Created a new proxy check job with id {Id}", entity.Id);

        // This might fail and we would have inconsistencies!
        // If that happens, remove the entity and rethrow
        try
        {
            var job = _jobFactory.FromOptions(entity.Id, apiUser.Id, jobOptions);
            _jobManager.AddJob(job);

            return await MapProxyCheckJobDto((ProxyCheckJob)job);
        }
        catch
        {
            await _jobRepo.DeleteAsync(entity);
            throw;
        }
    }

    /// <summary>
    /// Update a multi run job.
    /// </summary>
    [HttpPut("multi-run")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<MultiRunJobDto>> UpdateMultiRunJob(
        UpdateMultiRunJobDto dto, [FromServices] IValidator<UpdateMultiRunJobDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        // Make sure job is idle
        var job = GetJob<MultiRunJob>(dto.Id);

        if (job.Status is not JobStatus.Idle)
        {
            throw new ResourceInUseException(ErrorCode.JobNotIdle,
                $"Job {dto.Id} is not idle");
        }

        var entity = await GetEntityAsync(dto.Id);
        EnsureOwnership(entity);

        var jobOptions = _mapper.Map<MultiRunJobOptions>(dto);

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var wrapper = new JobOptionsWrapper { Options = jobOptions };
        entity.JobOptions = JsonConvert.SerializeObject(wrapper, jsonSettings);

        await _jobRepo.UpdateAsync(entity);

        var oldJob = _jobManager.Jobs.First(j => j.Id == dto.Id);

        var newJob = _jobFactory.FromOptions(
            dto.Id, entity.Owner?.Id ?? 0, jobOptions);

        _jobManager.RemoveJob(oldJob);
        _jobManager.AddJob(newJob);
        
        _logger.LogInformation("Updated the multi run job with id {Id}", dto.Id);

        return await MapMultiRunJobDto((MultiRunJob)newJob);
    }

    /// <summary>
    /// Update a proxy check job.
    /// </summary>
    [HttpPut("proxy-check")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<ProxyCheckJobDto>> UpdateProxyCheckJob(
        UpdateProxyCheckJobDto dto, [FromServices] IValidator<UpdateProxyCheckJobDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        // Make sure job is idle
        var job = GetJob<ProxyCheckJob>(dto.Id);

        if (job.Status is not JobStatus.Idle)
        {
            throw new ResourceInUseException(ErrorCode.JobNotIdle,
                $"Job {dto.Id} is not idle");
        }

        var entity = await GetEntityAsync(dto.Id);
        EnsureOwnership(entity);

        var jobOptions = _mapper.Map<ProxyCheckJobOptions>(dto);

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var wrapper = new JobOptionsWrapper { Options = jobOptions };
        entity.JobOptions = JsonConvert.SerializeObject(wrapper, jsonSettings);

        await _jobRepo.UpdateAsync(entity);

        var oldJob = _jobManager.Jobs.First(j => j.Id == dto.Id);

        var newJob = _jobFactory.FromOptions(
            dto.Id, entity.Owner?.Id ?? 0, jobOptions);

        _jobManager.RemoveJob(oldJob);
        _jobManager.AddJob(newJob);
        
        _logger.LogInformation("Updated the proxy check job with id {Id}", dto.Id);

        return await MapProxyCheckJobDto((ProxyCheckJob)newJob);
    }

    /// <summary>
    /// Get the custom user inputs that can be set in a given multi run job for
    /// the currently selected config.
    /// </summary>
    [HttpGet("multi-run/custom-inputs")]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<CustomInputQuestionDto>> GetCustomInputs(int id)
    {
        var job = GetJob<MultiRunJob>(id);

        if (job.Config is null)
        {
            throw new BadRequestException(
                ErrorCode.InvalidJobConfiguration,
                $"The job with id {id} is missing a config");
        }

        return Ok(job.Config.Settings.InputSettings.CustomInputs.Select(i =>
            new CustomInputQuestionDto {
                Description = i.Description, DefaultAnswer = i.DefaultAnswer, VariableName = i.VariableName,
                CurrentAnswer = job.CustomInputsAnswers.TryGetValue(i.VariableName, out var answer) ? answer : null
            }));
    }

    /// <summary>
    /// Set the values of custom inputs in a multi run job for the
    /// currently selected config.
    /// </summary>
    [HttpPatch("multi-run/custom-inputs")]
    [MapToApiVersion("1.0")]
    public ActionResult SetCustomInputs(CustomInputsDto dto)
    {
        var job = GetJob<MultiRunJob>(dto.JobId);

        foreach (var input in dto.Answers)
        {
            job.CustomInputsAnswers[input.VariableName] = input.Answer;
        }
        
        _logger.LogInformation("Set custom inputs for job {Id}", dto.JobId);

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

        await _jobRepo.DeleteAsync(entity);
        _jobManager.RemoveJob(job);
        
        _logger.LogInformation("Deleted job with id {Id}", id);

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
        int deletedCount;

        // If any job not idle, throw!
        var notIdleJobs = _jobManager.Jobs
            .Where(j => CanSee(apiUser, j) && j.Status != JobStatus.Idle);

        if (notIdleJobs.Any())
        {
            throw new ResourceInUseException(ErrorCode.JobNotIdle,
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

            await _jobRepo.DeleteAsync(entities);

            foreach (var job in _jobManager.Jobs.Where(j => j.OwnerId == apiUser.Id).ToList())
            {
                _jobManager.RemoveJob(job);
            }
        }
        
        _logger.LogInformation("Deleted {DeletedCount} jobs", deletedCount);

        return new AffectedEntriesDto { Count = deletedCount };
    }

    /// <summary>
    /// Get the full debugger log of a hit. Note that bot log must
    /// be enabled in the settings, or it will be blank.
    /// </summary>
    [TypeFilter<AdminFilter>]
    [HttpGet("multi-run/hit-log")]
    [MapToApiVersion("1.0")]
    public ActionResult<MrjHitLogDto> GetHitLog(int jobId, string hitId)
    {
        var job = GetJob(jobId);
        EnsureOwnership(job);

        if (job is not MultiRunJob mrJob)
        {
            throw new BadRequestException(
                ErrorCode.InvalidJobType,
                $"The job with id {jobId} is not a multi run job");
        }

        var hit = mrJob.Hits.Find(h => h.Id == hitId);

        if (hit is null)
        {
            throw new EntryNotFoundException(ErrorCode.HitNotFound,
                hitId, nameof(MultiRunJob.Hits));
        }

        return new MrjHitLogDto { Log = hit.BotLogger?.Entries.ToList() };
    }

    /// <summary>
    /// Start a job.
    /// </summary>
    [HttpPost("start")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> StartJob(JobCommandDto dto)
    {
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        if (dto.Wait)
        {
            await job.Start();
        }
        else
        {
            job.Start().Forget(
                e => _logger.LogError(
                    "Error while starting job {JobId}: {Message}", dto.JobId, e.Message));
        }
        
        _logger.LogInformation("Started job {JobId}", dto.JobId);

        return Ok();
    }

    /// <summary>
    /// Stop a job.
    /// </summary>
    [HttpPost("stop")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> StopJob(JobCommandDto dto)
    {
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        if (dto.Wait)
        {
            await job.Stop();
        }
        else
        {
            job.Stop().Forget(
                e => _logger.LogError(
                    "Error while stopping job {JobId}: {Message}", dto.JobId, e.Message));
        }
        
        _logger.LogInformation("Stopped job {JobId}", dto.JobId);

        return Ok();
    }

    /// <summary>
    /// Pause a job.
    /// </summary>
    [HttpPost("pause")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> PauseJob(JobCommandDto dto)
    {
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        if (dto.Wait)
        {
            await job.Pause();
        }
        else
        {
            job.Pause().Forget(
                e => _logger.LogError(
                    "Error while pausing job {JobId}: {Message}", dto.JobId, e.Message));
        }
        
        _logger.LogInformation("Paused job {JobId}", dto.JobId);

        return Ok();
    }

    /// <summary>
    /// Resume a paused job.
    /// </summary>
    [HttpPost("resume")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> ResumeJob(JobCommandDto dto)
    {
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        if (dto.Wait)
        {
            await job.Resume();
        }
        else
        {
            job.Resume().Forget(
                e => _logger.LogError(
                    "Error while resuming job {JobId}: {Message}", dto.JobId, e.Message));
        }
        
        _logger.LogInformation("Resumed job {JobId}", dto.JobId);

        return Ok();
    }

    /// <summary>
    /// Abort a job.
    /// </summary>
    [HttpPost("abort")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> AbortJob(JobCommandDto dto)
    {
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        if (dto.Wait)
        {
            await job.Abort();
        }
        else
        {
            job.Abort().Forget(
                e => _logger.LogError(
                    "Error while aborting job {JobId}: {Message}", dto.JobId, e.Message));
        }

        _logger.LogInformation("Aborted job {JobId}", dto.JobId);
        
        return Ok();
    }

    /// <summary>
    /// Skip a job's waiting time.
    /// </summary>
    [HttpPost("skip-wait")]
    [MapToApiVersion("1.0")]
    public ActionResult SkipWaitJob(JobCommandDto dto)
    {
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        job.SkipWait();
        _logger.LogInformation("Skipped wait for job {JobId}", dto.JobId);
        return Ok();
    }

    /// <summary>
    /// Change the number of bots in a job.
    /// </summary>
    [HttpPost("change-bots")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult> ChangeBots(ChangeBotsDto dto,
        [FromServices] IValidator<ChangeBotsDto> validator)
    {
        await validator.ValidateAndThrowAsync(dto);
        
        var job = GetJob(dto.JobId);
        EnsureOwnership(job);

        switch (job)
        {
            case MultiRunJob mrj:
                await mrj.ChangeBots(dto.Bots);
                break;

            case ProxyCheckJob pcj:
                await pcj.ChangeBots(dto.Bots);
                break;

            default:
                throw new NotSupportedException();
        }
        
        _logger.LogInformation(
            "Changed the number of bots for job {JobId} to {Bots}", dto.JobId, dto.Bots);

        return Ok();
    }
    
    /// <summary>
    /// Get the details of all bots in a multi run job.
    /// </summary>
    [HttpGet("multi-run/bot-details")]
    [MapToApiVersion("1.0")]
    public ActionResult<IEnumerable<BotDetailsDto>> GetBotDetails(int jobId)
    {
        var job = GetJob<MultiRunJob>(jobId);
        
        return job.CurrentBotDatas
            .Take(job.Bots)
            .Where(d => d is not null)
            .Select(d => new BotDetailsDto
            {
                Id = d.BOTNUM,
                Data = d.Line.Data,
                Proxy = d.Proxy?.ToString(),
                Info = d.ExecutionInfo
            }).ToList();
    }
    
    /// <summary>
    /// Get the record of a config and wordlist combination. If no record
    /// exists, a fake one with checkpoint 0 will be returned.
    /// </summary>
    [HttpGet("multi-run/record")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<RecordDto>> GetRecord(string configId, int wordlistId)
    {
        var record = await _recordRepo.GetAll()
            .FirstOrDefaultAsync(r => r.ConfigId == configId && r.WordlistId == wordlistId);

        if (record is null)
        {
            return new RecordDto
            {
                ConfigId = configId,
                WordlistId = wordlistId,
                Checkpoint = 0
            };
        }
        
        return _mapper.Map<RecordDto>(record);
    }

    private Job GetJob(int id)
    {
        var job = _jobManager.Jobs.FirstOrDefault(j => j.Id == id);

        if (job is null)
        {
            throw new EntryNotFoundException(ErrorCode.JobNotFound,
                id, nameof(JobManagerService));
        }

        return job;
    }

    private T GetJob<T>(int id) where T : Job
    {
        var job = GetJob(id);
        EnsureOwnership(job);

        if (job is not T typedJob)
        {
            throw new BadRequestException(
                ErrorCode.InvalidJobType,
                $"The job with id {id} is not of type {typeof(T).Name}");
        }

        return typedJob;
    }

    private async Task<JobEntity> GetEntityAsync(int id)
    {
        var entity = await _jobRepo.GetAsync(id);

        if (entity is null)
        {
            throw new EntryNotFoundException(ErrorCode.JobNotFound,
                id, nameof(IJobRepository));
        }

        return entity;
    }

    private bool CanSee(ApiUser apiUser, Job job)
        => apiUser.Role is UserRole.Admin || job.OwnerId == apiUser.Id;

    private void EnsureOwnership(Job job)
    {
        var apiUser = HttpContext.GetApiUser();

        // If the user is trying to access a job that is not owned
        // by them, throw a not found exception.
        if (apiUser.Role is UserRole.Guest && apiUser.Id != job.OwnerId)
        {
            _logger.LogWarning("Guest user {Username} tried to access a job not owned by them",
                apiUser.Username);

            throw new EntryNotFoundException(ErrorCode.JobNotFound,
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
            _logger.LogWarning("Guest user {Username} tried to access a job not owned by them",
                apiUser.Username);

            throw new EntryNotFoundException(ErrorCode.JobNotFound,
                entity.Id, nameof(IJobRepository));
        }
    }

    private async Task<ProxyCheckJobDto> MapProxyCheckJobDto(ProxyCheckJob job)
    {
        var checkOutput = job.ProxyOutput switch {
            DatabaseProxyCheckOutput => "database",
            _ => throw new NotImplementedException()
        };

        TimeStartConditionDto startCondition = job.StartCondition switch {
            RelativeTimeStartCondition r => new RelativeTimeStartConditionDto {
                PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType<RelativeTimeStartConditionDto>()!,
                StartAfter = r.StartAfter
            },
            AbsoluteTimeStartCondition a => new AbsoluteTimeStartConditionDto {
                PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType<AbsoluteTimeStartConditionDto>()!,
                StartAt = a.StartAt
            },
            _ => throw new NotImplementedException()
        };

        var entity = await GetEntityAsync(job.Id);
        EnsureOwnership(entity);

        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        var jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(
            entity.JobOptions, jsonSettings)?.Options;

        if (jobOptions is null)
        {
            throw new ApiException(ErrorCode.InvalidJobConfiguration, "The job options are null");
        }

        if (jobOptions is not ProxyCheckJobOptions pcjJobOptions)
        {
            throw new ApiException(ErrorCode.InvalidJobType, "Invalid job options type");
        }

        var groupName = "All";

        if (pcjJobOptions.GroupId != -1)
        {
            var proxyGroup = await _proxyGroupRepo.GetAsync(pcjJobOptions.GroupId);

            if (proxyGroup is not null)
            {
                groupName = proxyGroup.Name;
            }
        }

        return new ProxyCheckJobDto {
            Id = job.Id,
            Name = job.Name,
            StartCondition = startCondition,
            StartTime = job.StartTime,
            OwnerId = job.OwnerId,
            Type = GetJobType(job),
            Status = job.Status,
            Bots = job.Bots,
            GroupId = pcjJobOptions.GroupId,
            GroupName = groupName,
            CheckOnlyUntested = job.CheckOnlyUntested,
            Target = new ProxyCheckTargetDto { Url = job.Url, SuccessKey = job.SuccessKey },
            CheckOutput = checkOutput,
            Tested = job.Tested,
            Working = job.Working,
            NotWorking = job.NotWorking,
            Total = job.Total,
            TimeoutMilliseconds = (int)job.Timeout.TotalMilliseconds,
            CPM = job.CPM,
            Elapsed = job.Elapsed,
            Remaining = job.Remaining,
            Progress = job.Progress < 0 ? 0 : job.Progress
        };
    }

    private async Task<MultiRunJobDto> MapMultiRunJobDto(MultiRunJob job)
    {
        var dataPoolInfo = job.DataPool switch {
            WordlistDataPool w => $"{w.Wordlist?.Name} (Wordlist)",
            CombinationsDataPool c => $"Combinations of {c.CharSet} with length {c.Length}",
            RangeDataPool r => $"Range from {r.Start} with amount {r.Amount} and step {r.Step} (padding {r.Pad})",
            InfiniteDataPool => "Infinite",
            FileDataPool f => $"{f.FileName} (File)",
            _ => throw new NotImplementedException()
        };

        var proxySources = await Task.WhenAll(job.ProxySources.Select(async s => s switch {
            GroupProxySource g => $"{await GetProxyGroupName(g.GroupId)} (Group)",
            FileProxySource f => $"{f.FileName} (File)",
            RemoteProxySource r => $"{r.Url} (Remote)",
            _ => throw new NotImplementedException()
        }));

        var hitOutputs = job.HitOutputs.Select(o => o switch {
            DatabaseHitOutput => "Database",
            FileSystemHitOutput f => $"{f.BaseDir} (File System)",
            DiscordWebhookHitOutput => "Discord Webhook",
            TelegramBotHitOutput => "Telegram bot",
            CustomWebhookHitOutput => "Custom Webhook",
            _ => throw new NotImplementedException()
        }).ToList();

        TimeStartConditionDto startCondition = job.StartCondition switch {
            RelativeTimeStartCondition r => new RelativeTimeStartConditionDto {
                PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType<RelativeTimeStartConditionDto>()!,
                StartAfter = r.StartAfter
            },
            AbsoluteTimeStartCondition a => new AbsoluteTimeStartConditionDto {
                PolyTypeName = PolyDtoCache.GetPolyTypeNameFromType<AbsoluteTimeStartConditionDto>()!,
                StartAt = a.StartAt
            },
            _ => throw new NotImplementedException()
        };

        return new MultiRunJobDto {
            Id = job.Id,
            Name = job.Name,
            StartCondition = startCondition,
            StartTime = job.StartTime,
            OwnerId = job.OwnerId,
            Type = GetJobType(job),
            Status = job.Status,
            Config =
                job.Config is not null
                    ? new JobConfigDto {
                        Id = job.Config.Id,
                        Name = job.Config.Metadata.Name,
                        Author = job.Config.Metadata.Author,
                        Base64Image = job.Config.Metadata.Base64Image,
                        NeedsProxies = job.Config.Settings.ProxySettings.UseProxies
                    }
                    : null,
            DataPoolInfo = dataPoolInfo,
            Bots = job.Bots,
            Skip = job.Skip,
            ProxyMode = job.ProxyMode,
            ProxySources = proxySources.ToList(),
            HitOutputs = hitOutputs,
            DataStats =
                new MrjDataStatsDto {
                    Hits = job.DataHits,
                    Custom = job.DataCustom,
                    Fails = job.DataFails,
                    Invalid = job.DataInvalid,
                    Retried = job.DataRetried,
                    Banned = job.DataBanned,
                    Errors = job.DataErrors,
                    ToCheck = job.DataToCheck,
                    Total = job.DataPool.Size,
                    Tested = job.DataTested
                },
            ProxyStats =
                new MrjProxyStatsDto {
                    Total = job.ProxiesTotal, Alive = job.ProxiesAlive, Bad = job.ProxiesBad, Banned = job.ProxiesBanned
                },
            CPM = job.CPM,
            CaptchaCredit = job.CaptchaCredit,
            Elapsed = job.Elapsed,
            Remaining = job.Remaining,
            Progress = job.Progress < 0 ? 0 : job.Progress,
            Hits = job.Hits.Select(h => new MrjHitDto {
                Id = h.Id,
                Date = h.Date,
                Type = h.Type,
                Data = h.DataString,
                Proxy = h.Proxy is not null 
                    ? new MrjProxy
                    {
                        Type = h.Proxy.Type,
                        Host = h.Proxy.Host,
                        Port = h.Proxy.Port,
                        Username = h.Proxy.Username,
                        Password = h.Proxy.Password,
                    }
                    : null,
                CapturedData = h.CapturedDataString
            }).ToList()
        };
    }

    private async Task<string> GetProxyGroupName(int id)
    {
        if (id == -1)
        {
            return "All";
        }

        var proxyGroup = await _proxyGroupRepo.GetAsync(id);
        return proxyGroup is null ? "Invalid" : proxyGroup.Name;
    }

    private static JobType GetJobType(Job job) =>
        job switch {
            MultiRunJob => JobType.MultiRun,
            ProxyCheckJob => JobType.ProxyCheck,
            _ => throw new NotImplementedException()
        };
}
