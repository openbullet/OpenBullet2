using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using OpenBullet2.Core.Entities;
using OpenBullet2.Core.Helpers;
using OpenBullet2.Core.Models.Data;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Core.Services;

/// <summary>
/// Manages multiple jobs.
/// </summary>
public class JobManagerService : IDisposable
{
    /// <summary>
    /// The list of all created jobs.
    /// </summary>
    public IEnumerable<Job> Jobs => _jobs;
    private readonly List<Job> _jobs = [];

    private readonly SemaphoreSlim _jobSemaphore = new(1, 1);
    private readonly SemaphoreSlim _recordSemaphore = new(1, 1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobManagerService> _logger;

    public JobManagerService(
        IServiceScopeFactory scopeFactory,
        JobFactoryService jobFactory,
        ILogger<JobManagerService>? logger = null)
    {
        _logger = logger ?? NullLogger<JobManagerService>.Instance;
        using var scope = scopeFactory.CreateScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();

        // Restore jobs from the database
        var entities = jobRepo.GetAll().Include(j => j.Owner).ToList();
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        foreach (var entity in entities)
        {
            if (string.IsNullOrEmpty(entity.JobOptions))
            {
                continue;
            }

            try
            {
                // Convert old namespaces to support old databases
                if (entity.JobOptions.Contains("OpenBullet2.Models") || entity.JobOptions.Contains(", OpenBullet2\""))
                {
                    entity.JobOptions = entity.JobOptions
                        .Replace("OpenBullet2.Models", "OpenBullet2.Core.Models")
                        .Replace(", OpenBullet2\"", ", OpenBullet2.Core\"");

                    jobRepo.UpdateAsync(entity).Wait();
                }

                var wrapper = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, jsonSettings);
                if (wrapper?.Options is null)
                {
                    continue;
                }

                var options = wrapper.Options;
                var job = jobFactory.FromOptions(entity.Id, entity.Owner == null ? 0 : entity.Owner.Id, options);
                AddJob(job);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipped restoring job {JobId}", entity.Id);
            }
        }

        _scopeFactory = scopeFactory;
    }

    public void AddJob(Job job)
    {
        _jobs.Add(job);
        _logger.LogDebug("Added job {JobId} of type {JobType} to JobManagerService", job.Id, job.GetType().Name);

        if (job is MultiRunJob mrj)
        {
            mrj.OnCompleted += SaveRecord;
            mrj.OnTimerTick += SaveRecord;
            mrj.OnCompleted += SaveMultiRunJobOptionsAsync;
            mrj.OnTimerTick += SaveMultiRunJobOptionsAsync;
            mrj.OnBotsChanged += SaveMultiRunJobOptionsAsync;
        }
    }

    public void RemoveJob(Job job)
    {
        _jobs.Remove(job);
        UnbindEvents(job);

        try
        {
            job.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to dispose removed job {JobId}", job.Id);
        }

        _logger.LogDebug("Removed job {JobId} of type {JobType} from JobManagerService", job.Id, job.GetType().Name);
    }

    public void Clear()
    {
        foreach (var job in _jobs.ToList())
        {
            UnbindEvents(job);

            try
            {
                job.Dispose();
            }
            catch
            {
                // ignored
            }
        }

        _jobs.Clear();
    }

    // Saves the record for a MultiRunJob in the IRecordRepository. Thread safe.
    private async void SaveRecord(object? sender, EventArgs e)
    {
        using var scope = _scopeFactory.CreateScope();
        var recordRepo = scope.ServiceProvider.GetRequiredService<IRecordRepository>();

        if (sender is not MultiRunJob job || job.DataPool is not WordlistDataPool pool || job.Config is null)
        {
            return;
        }

        await _recordSemaphore.WaitAsync();

        try
        {
            var record = await recordRepo.GetAll()
                    .FirstOrDefaultAsync(r => r.ConfigId == job.Config.Id && r.WordlistId == pool.Wordlist.Id);

            var checkpoint = job.Status == JobStatus.Idle
                ? MultiRunJobCheckpoint.GetNextSkip(job.Skip, job.DataTested, pool.Size)
                : job.Skip + job.DataTested;

            if (record == null)
            {
                await recordRepo.AddAsync(new RecordEntity
                {
                    ConfigId = job.Config.Id,
                    WordlistId = pool.Wordlist.Id,
                    Checkpoint = checkpoint
                });
            }
            else
            {
                record.Checkpoint = checkpoint;
                await recordRepo.UpdateAsync(record);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to save record for multirun job {JobId}", job.Id);
        }
        finally
        {
            _recordSemaphore.Release();
        }
    }

    private async void SaveMultiRunJobOptionsAsync(object? sender, EventArgs e)
    {
        if (sender is not MultiRunJob job)
        {
            return;
        }

        await SaveMultiRunJobOptionsAsync(job);
    }

    // Saves the options for a MultiRunJob in the IJobRepository. Thread safe.
    public async Task SaveMultiRunJobOptionsAsync(MultiRunJob job)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();

        await _jobSemaphore.WaitAsync();

        try
        {
            var entity = await jobRepo.GetAsync(job.Id);

            if (entity == null || entity.JobOptions == null)
            {
                _logger.LogDebug("Skipped job options save for job {JobId} because the entity or job options were null",
                    job.Id);
                return;
            }

            // Deserialize and unwrap the job options
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var wrapper = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, settings);
            if (wrapper?.Options is not MultiRunJobOptions options)
            {
                _logger.LogDebug("Skipped job options save for job {JobId} because deserialization failed", job.Id);
                return;
            }

            // Check if it's valid
            if (string.IsNullOrEmpty(options.ConfigId))
            {
                _logger.LogDebug("Skipped job options save for job {JobId} because ConfigId was null", job.Id);
                return;
            }

            if (options.DataPool is WordlistDataPoolOptions x && x.WordlistId == -1)
            {
                _logger.LogDebug("Skipped job options save for job {JobId} because WordlistId was -1", job.Id);
                return;
            }

            // Update the skip (if not idle, also add the currently tested ones) and the bots
            options.Skip = job.Status == JobStatus.Idle
                ? MultiRunJobCheckpoint.GetNextSkip(job.Skip, job.DataTested, job.DataPool?.Size ?? 0)
                : job.Skip + job.DataTested;

            options.Bots = job.Bots;
            options.CustomInputsAnswers = CustomInputAnswerHelper.FilterAnswers(job.Config, job.CustomInputsAnswers);

            // Wrap and serialize again
            var newWrapper = new JobOptionsWrapper { Options = options };
            entity.JobOptions = JsonConvert.SerializeObject(newWrapper, settings);

            // Update the job
            await jobRepo.UpdateAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to save multirun job options for job {JobId}", job.Id);
        }
        finally
        {
            _jobSemaphore.Release();
        }
    }

    private void UnbindEvents(Job job)
    {
        if (job is MultiRunJob mrj)
        {
            try
            {
                mrj.OnCompleted -= SaveRecord;
                mrj.OnTimerTick -= SaveRecord;
                mrj.OnCompleted -= SaveMultiRunJobOptionsAsync;
                mrj.OnTimerTick -= SaveMultiRunJobOptionsAsync;
                mrj.OnBotsChanged -= SaveMultiRunJobOptionsAsync;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Failed to unbind events for job {JobId}", mrj.Id);
            }
        }
    }

    public void Dispose()
    {
        Clear();
        _jobSemaphore.Dispose();
        _recordSemaphore.Dispose();
    }
}
