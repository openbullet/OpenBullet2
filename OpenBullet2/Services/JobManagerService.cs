using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using RuriLib.Models.Jobs;
using System.Collections.Generic;
using System.Linq;

namespace OpenBullet2.Services
{
    public class JobManagerService
    {
        public JobManagerService(IJobRepository jobRepo, JobFactoryService jobFactory)
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
        }

        public List<Job> Jobs { get; } = new List<Job>();
    }
}
