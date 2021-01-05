using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Entities;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using RuriLib.Models.Data.DataPools;
using RuriLib.Models.Jobs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Services
{
    public class JobManagerService
    {
        public List<Job> Jobs { get; } = new List<Job>();

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

            this.recordRepo = recordRepo;
        }

        public async Task SaveRecord(MultiRunJob job)
        {
            if (job.DataPool is WordlistDataPool pool)
            {
                var record = recordRepo.GetAll()
                    .FirstOrDefault(r => r.ConfigId == job.Config.Id && r.WordlistId == pool.Wordlist.Id);

                if (record == null)
                {
                    await recordRepo.Add(new RecordEntity
                    {
                        ConfigId = job.Config.Id,
                        WordlistId = pool.Wordlist.Id,
                        Checkpoint = job.Skip + job.DataTested
                    });
                }
                else
                {
                    record.Checkpoint = job.Skip + job.DataTested;
                    await recordRepo.Update(record);
                }
            }
        }
    }
}
