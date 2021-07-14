using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Entities;
using OpenBullet2.Models.Data;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class JobManagerService
    {
        public List<Job> Jobs { get; } = new List<Job>();

        private readonly SemaphoreSlim jobSemaphore = new(1, 1);
        private readonly SemaphoreSlim recordSemaphore = new(1, 1);
        private readonly IJobRepository jobRepo;
        private readonly IRecordRepository recordRepo;

        public JobManagerService(IJobRepository jobRepo, JobFactoryService jobFactory, IRecordRepository recordRepo)
        {
            // Restore jobs from the database
            var entities = jobRepo.GetAll().Include(j => j.Owner).ToList();
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var entity in entities)
            {
                var options = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, jsonSettings).Options;
                var job = jobFactory.FromOptions(entity.Id, entity.Owner == null ? 0 : entity.Owner.Id, options);
                Jobs.Add(job);
            }

            this.jobRepo = jobRepo;
            this.recordRepo = recordRepo;
        }

        public async Task SaveRecord(MultiRunJob job)
        {
            if (job.DataPool is not WordlistDataPool pool)
            {
                return;
            }

            await recordSemaphore.WaitAsync();

            try
            {
                var record = await recordRepo.GetAll()
                        .FirstOrDefaultAsync(r => r.ConfigId == job.Config.Id && r.WordlistId == pool.Wordlist.Id);

                var checkpoint = job.Status == JobStatus.Idle
                    ? job.Skip
                    : job.Skip + job.DataTested;

                if (record == null)
                {
                    await recordRepo.Add(new RecordEntity
                    {
                        ConfigId = job.Config.Id,
                        WordlistId = pool.Wordlist.Id,
                        Checkpoint = checkpoint
                    });
                }
                else
                {
                    record.Checkpoint = checkpoint;
                    await recordRepo.Update(record);
                }
            }
            catch
            {

            }
            finally
            {
                recordSemaphore.Release();
            }
        }

        public async Task SaveJobOptions(MultiRunJob job)
        {
            await jobSemaphore.WaitAsync();

            try
            {
                var entity = await jobRepo.Get(job.Id);

                if (entity == null || entity.JobOptions == null)
                {
                    Console.WriteLine("Skipped job options save because Job (or JobOptions) was null");
                    return;
                }

                // Deserialize and unwrap the job options
                var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
                var wrapper = JsonConvert.DeserializeObject<JobOptionsWrapper>(entity.JobOptions, settings);
                var options = ((MultiRunJobOptions)wrapper.Options);

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
                await jobRepo.Update(entity);
            }
            catch
            {

            }
            finally
            {
                jobSemaphore.Release();
            }
        }
    }
}
