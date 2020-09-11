using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobCloner
    {
        [Inject] IJobRepository JobRepo { get; set; }
        [Inject] JobManagerService Manager { get; set; }
        [Inject] NavigationManager Nav { get; set; }
        [Inject] JobFactoryService JobFactory { get; set; }

        [Parameter] public int JobId { get; set; }
        JobType jobType;
        JobOptions jobOptions;
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        protected override async Task OnInitializedAsync()
        {
            var factory = new JobOptionsFactory();
            var jobEntity = await JobRepo.Get(JobId);
            var oldOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(jobEntity.JobOptions, settings).Options;
            jobOptions = factory.CloneExistant(oldOptions);
            jobType = jobEntity.JobType;
        }

        private async Task Clone()
        {
            var wrapper = new JobOptionsWrapper { Options = jobOptions };
            var entity = new JobEntity
            {
                CreationDate = DateTime.Now,
                JobType = jobType,
                JobOptions = JsonConvert.SerializeObject(wrapper, settings)
            };

            await JobRepo.Add(entity);

            // Get the entity that was just added in order to get its ID
            entity = await JobRepo.GetAll().OrderByDescending(e => e.Id).FirstAsync();

            try
            {
                var job = JobFactory.FromOptions(entity.Id, jobOptions);

                Manager.Jobs.Add(job);
                Nav.NavigateTo($"job/{job.Id}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
