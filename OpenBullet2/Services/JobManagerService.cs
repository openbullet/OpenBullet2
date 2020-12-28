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
            var entries = jobRepo.GetAll().ToList();
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var entry in entries)
            {
                var options = JsonConvert.DeserializeObject<JobOptionsWrapper>(entry.JobOptions, jsonSettings).Options;
                var job = jobFactory.FromOptions(entry.Id, entry.Owner.Id, options);
                Jobs.Add(job);
            }
        }

        public List<Job> Jobs { get; } = new List<Job>();
    }
}
