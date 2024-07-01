using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenBullet2.Core.Entities;
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
    private readonly List<Job> _jobs = new();

    private readonly SemaphoreSlim _jobSemaphore = new(1, 1);
    private readonly SemaphoreSlim _recordSemaphore = new(1, 1);
    private readonly IServiceScopeFactory _scopeFactory;

    public JobManagerService(IServiceScopeFactory scopeFactory, JobFactoryService jobFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var jobRepo = scope.ServiceProvider.GetRequiredService<IJobRepository>();

        // Restore jobs from the database
        var entities = jobRepo.GetAll().Include(j => j.Owner).ToList();
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        foreach (var entity in entities)
        {
            // Convert old namespaces to support old databases
            if (entity.JobOptions.Contains("OpenBullet2.Models") || entity.JobOptions.Contains(", OpenBullet2\""))
            {
                entity.JobOptions = entity.JobOptions
                    .Replace("OpenBullet2.Models", "OpenBullet2.Core.Models")
                    .Replace(", OpenBullet2\"", ", OpenBullet2.Core\"");

                jobRepo.UpdateAsync(entity).Wait();
            }

            var options = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, jsonSettings).Options;
            var job = jobFactory.FromOptions(entity.Id, entity.Owner == null ? 0 : entity.Owner.Id, options);
            AddJob(job);
        }

        _scopeFactory = scopeFactory;
    }

    public void AddJob(Job job)
    {
        _jobs.Add(job);

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
            catch
            {

            }
        }
    }

    public void Clear()
    {
        UnbindAllEvents();
        _jobs.Clear();
    }

    // Saves the record for a MultiRunJob in the IRecordRepository. Thread safe.
    private async void SaveRecord(object sender, EventArgs e)
    {
        using var scope = _scopeFactory.CreateScope();
        var recordRepo = scope.ServiceProvider.GetRequiredService<IRecordRepository>();

        if (sender is not MultiRunJob job || job.DataPool is not WordlistDataPool pool)
        {
            return;
        }

        await _recordSemaphore.WaitAsync();

        try
        {
            var record = await recordRepo.GetAll()
                    .FirstOrDefaultAsync(r => r.ConfigId == job.Config.Id && r.WordlistId == pool.Wordlist.Id);

            var checkpoint = job.Status == JobStatus.Idle
                ? job.Skip
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
        catch
        {

        }
        finally
        {
            _recordSemaphore.Release();
        }
    }

    private async void SaveMultiRunJobOptionsAsync(object sender, EventArgs e)
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
                Console.WriteLine("Skipped job options save because Job (or JobOptions) was null");
                return;
            }

            // Deserialize and unwrap the job options
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var wrapper = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, settings);
            var options = (MultiRunJobOptions)wrapper.Options;

            // Check if it's valid
            if (string.IsNullOrEmpty(options.ConfigId))
            {
                Console.WriteLine("Skipped job options save because ConfigId was null");
                return;
            }

            if (options.DataPool is WordlistDataPoolOptions x && x.WordlistId == -1)
            {
                Console.WriteLine("Skipped job options save because WordlistId was -1");
                return;
            }

            // Update the skip (if not idle, also add the currently tested ones) and the bots
            options.Skip = job.Status == JobStatus.Idle
                ? job.Skip
                : job.Skip + job.DataTested;

            options.Bots = job.Bots;

            // Wrap and serialize again
            var newWrapper = new JobOptionsWrapper { Options = options };
            entity.JobOptions = JsonConvert.SerializeObject(newWrapper, settings);

            // Update the job
            await jobRepo.UpdateAsync(entity);
        }
        catch
        {

        }
        finally
        {
            _jobSemaphore.Release();
        }
    }

    private void UnbindAllEvents()
    {
        foreach (var job in _jobs)
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
                catch
                {

                }
            }
        }
    }

    public void Dispose() => UnbindAllEvents();
}
