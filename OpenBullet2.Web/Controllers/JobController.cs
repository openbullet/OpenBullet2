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
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Jobs;
using OpenBullet2.Core.Models.Proxies.Sources;
using RuriLib.Models.Proxies.ProxySources;
using OpenBullet2.Core.Models.Hits;
using RuriLib.Models.Hits.HitOutputs;

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

        var dtos = new List<MultiRunJobOverviewDto>();

        foreach (var job in jobs)
        {
            var dataPoolInfo = job.DataPool switch
            {
                WordlistDataPool w => $"{w.Wordlist?.Name} (Wordlist)",
                CombinationsDataPool => "Combinations",
                InfiniteDataPool => "Infinite",
                RangeDataPool => "Range",
                FileDataPool f => $"{f.FileName} (File)",
                _ => throw new NotImplementedException()
            };

            var dto = new MultiRunJobOverviewDto
            {
                Id = job.Id,
                OwnerId = job.OwnerId,
                Status = job.Status,
                Name = job.Name,
                ConfigName = job.Config?.Metadata.Name,
                DataPoolInfo = dataPoolInfo,
                DataHits = job.DataHits,
                DataCustom = job.DataCustom,
                DataToCheck = job.DataToCheck,
                DataTotal = job.DataPool.Size,
                DataTested = job.Status is JobStatus.Idle ? job.Skip : job.DataTested + job.Skip,
                CPM = job.CPM,
                Progress = job.Progress == -1 ? 0 : job.Progress
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
        var job = GetJob<MultiRunJob>(id);
        return MapMultiRunJobDto(job);
    }

    /// <summary>
    /// Get the details of a proxy check job.
    /// </summary>
    [HttpGet("proxy-check")]
    [MapToApiVersion("1.0")]
    public ActionResult<ProxyCheckJobDto> GetProxyCheckJob(int id)
    {
        var job = GetJob<ProxyCheckJob>(id);
        return _mapper.Map<ProxyCheckJobDto>(job);
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
        if (id == -1)
        {
            var options = JobOptionsFactory.CreateNew(JobType.MultiRun);
            return _mapper.Map<MultiRunJobOptionsDto>(options);
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

        if (jobOptions is not MultiRunJobOptions mrjJobOptions)
        {
            throw new Exception("Invalid job options type");
        }

        return _mapper.Map<MultiRunJobOptionsDto>(mrjJobOptions);
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
        var apiUser = HttpContext.GetApiUser();
        var jobOptions = _mapper.Map<MultiRunJobOptions>(dto);

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
            JobType = JobType.MultiRun,
            JobOptions = JsonConvert.SerializeObject(wrapper, jsonSettings)
        };

        await _jobRepo.Add(entity);

        // This might fail and we would have inconsistencies!
        // If that happens, remove the entity and rethrow
        try
        {
            var job = _jobFactory.FromOptions(entity.Id, apiUser.Id, jobOptions);
            _jobManager.AddJob(job);

            return MapMultiRunJobDto((MultiRunJob)job);
        }
        catch
        {
            await _jobRepo.Delete(entity);
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
        // Make sure job is idle
        var job = GetJob<MultiRunJob>(dto.Id);
        
        if (job.Status is not JobStatus.Idle)
        {
            throw new ResourceInUseException(ErrorCode.JOB_NOT_IDLE,
                $"Job {dto.Id} is not idle");
        }

        var entity = await GetEntityAsync(dto.Id);
        EnsureOwnership(entity);

        var jobOptions = _mapper.Map<MultiRunJobOptions>(dto);

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

        return MapMultiRunJobDto((MultiRunJob)newJob);
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
        var job = GetJob<ProxyCheckJob>(dto.Id);

        if (job.Status is not JobStatus.Idle)
        {
            throw new ResourceInUseException(ErrorCode.JOB_NOT_IDLE,
                $"Job {dto.Id} is not idle");
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
                ErrorCode.INVALID_JOB_CONFIGURATION,
                $"The job with id {id} is missing a config");
        }

        return Ok(job.Config.Settings.InputSettings.CustomInputs.Select(i =>
        new CustomInputQuestionDto
        {
            Description = i.Description,
            DefaultAnswer = i.DefaultAnswer,
            VariableName = i.VariableName
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
        var job = GetJob<MultiRunJob>(dto.Id);

        foreach (var input in dto.Inputs)
        {
            job.CustomInputsAnswers[input.VariableName] = input.Answer;
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

    /// <summary>
    /// Get the full debugger log of a hit. Note that bot log must
    /// be enabled in the settings, or it will be blank.
    /// </summary>
    [HttpGet("multi-run/hit-log")]
    [MapToApiVersion("1.0")]
    public ActionResult<MRJHitLogDto> GetHitLog(int jobId, string hitId)
    {
        var job = GetJob(jobId);
        EnsureOwnership(job);

        if (job is not MultiRunJob mrJob)
        {
            throw new BadRequestException(
                ErrorCode.INVALID_JOB_TYPE,
                $"The job with id {jobId} is not a multi run job");
        }

        var hit = mrJob.Hits.FirstOrDefault(h => h.Id == hitId);

        if (hit is null)
        {
            throw new EntryNotFoundException(ErrorCode.HIT_NOT_FOUND,
                hitId, nameof(MultiRunJob.Hits));
        }

        return new MRJHitLogDto
        {
            Log = hit.BotLogger.Entries.ToList()
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

    private T GetJob<T>(int id) where T : Job
    {
        var job = GetJob(id);
        EnsureOwnership(job);

        if (job is not T typedJob)
        {
            throw new BadRequestException(
                ErrorCode.INVALID_JOB_TYPE,
                $"The job with id {id} is not of type {typeof(T).Name}");
        }

        return typedJob;
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
        => apiUser.Role is UserRole.Admin || job.OwnerId == apiUser.Id;

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

    private MultiRunJobDto MapMultiRunJobDto(MultiRunJob job)
    {
        var dataPoolInfo = job.DataPool switch
        {
            WordlistDataPool w => $"{w.Wordlist?.Name} (Wordlist)",
            CombinationsDataPool c => $"Combinations of {c.CharSet} with length {c.Length}",
            RangeDataPool r => $"Range from {r.Start} with amount {r.Amount} and step {r.Step} (padding {r.Pad})",
            InfiniteDataPool => "Infinite",
            FileDataPool f => $"{f.FileName} (File)",
            _ => throw new NotImplementedException()
        };

        var proxySources = job.ProxySources.Select(s => s switch
        {
            GroupProxySource g => $"{g.GroupId} (Group)",
            FileProxySource f => $"{f.FileName} (File)",
            RemoteProxySource r => $"{r.Url} (Remote)",
            _ => throw new NotImplementedException()
        }).ToList();

        var hitOutputs = job.HitOutputs.Select(o => o switch
        {
            DatabaseHitOutput => "Database",
            FileSystemHitOutput f => $"{f.BaseDir} (File System)",
            DiscordWebhookHitOutput => "Discord Webhook",
            TelegramBotHitOutput => "Telegram bot",
            CustomWebhookHitOutput => "Custom Webhook",
            _ => throw new NotImplementedException()
        }).ToList();

        return new MultiRunJobDto
        {
            Id = job.Id,
            OwnerId = job.OwnerId,
            Status = job.Status,
            Config = job.Config is not null ? new JobConfigDto
            {
                Id = job.Config.Id,
                Name = job.Config.Metadata.Name,
                Author = job.Config.Metadata.Author,
                Base64Image = job.Config.Metadata.Base64Image
            } : null,
            DataPoolInfo = dataPoolInfo,
            Bots = job.Bots,
            Skip = job.Skip,
            ProxyMode = job.ProxyMode,
            ProxySources = proxySources,
            HitOutputs = hitOutputs,
            DataStats = new MRJDataStatsDto
            {
                Hits = job.DataHits,
                Custom = job.DataCustom,
                Fails = job.DataFails,
                Invalid = job.DataInvalid,
                Retried = job.DataRetried,
                Banned = job.DataBanned,
                Errors = job.DataErrors,
                ToCheck = job.DataToCheck,
                Total = job.DataPool.Size,
                Tested = job.Status is JobStatus.Idle ? job.Skip : job.DataTested + job.Skip
            },
            ProxyStats = new MRJProxyStatsDto
            {
                Total = job.ProxiesTotal,
                Alive = job.ProxiesAlive,
                Bad = job.ProxiesBad,
                Banned = job.ProxiesBanned
            },
            CPM = job.CPM,
            CaptchaCredit = job.CaptchaCredit,
            Elapsed = job.Elapsed,
            Remaining = job.Remaining,
            Progress = job.Progress == -1 ? 0 : job.Progress,
            Hits = job.Hits.Select(h => new MRJHitDto
            {
                Id = h.Id,
                Date = h.Date,
                Type = h.Type,
                Data = h.DataString,
                CapturedData = h.CapturedDataString
            }).ToList()
        };
    }
}
