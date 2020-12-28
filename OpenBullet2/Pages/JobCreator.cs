using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Auth;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Jobs;
using RuriLib.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobCreator
    {
        [Inject] IJobRepository JobRepo { get; set; }
        [Inject] IGuestRepository GuestRepo { get; set; }
        [Inject] JobManagerService Manager { get; set; }
        [Inject] NavigationManager Nav { get; set; }
        [Inject] JobFactoryService JobFactory { get; set; }
        [Parameter] public string Type { get; set; }
        [Inject] AuthenticationStateProvider Auth { get; set; }

        JobType jobType;
        JobOptions jobOptions;
        private int uid = -1;

        protected override async Task OnInitializedAsync()
        {
            uid = await((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            Type ??= JobType.MultiRun.ToString();

            var factory = new JobOptionsFactory();
            jobType = Enum.Parse<JobType>(Type);
            jobOptions = factory.CreateNew(jobType);
        }

        private async Task Create()
        {
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var wrapper = new JobOptionsWrapper { Options = jobOptions };

            var entity = new JobEntity
            {
                Owner = await GuestRepo.Get(uid),
                CreationDate = DateTime.Now,
                JobType = jobType,
                JobOptions = JsonConvert.SerializeObject(wrapper, settings)
            };

            await JobRepo.Add(entity);

            // Get the entity that was just added in order to get its ID
            // entity = await JobRepo.GetAll().OrderByDescending(e => e.Id).FirstAsync();

            try
            {
                var job = JobFactory.FromOptions(entity.Id, entity.Owner == null ? 0 : entity.Owner.Id, jobOptions);

                Manager.Jobs.Add(job);
                Nav.NavigateTo($"job/{job.Id}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private JobType GetJobType(Job job)
        {
            return job switch
            {
                MultiRunJob _ => JobType.MultiRun,
                ProxyCheckJob _ => JobType.ProxyCheck,
                RipJob _ => JobType.Ripper,
                SpiderJob _ => JobType.Spider,
                SeleniumUnitTestJob _ => JobType.SeleniumUnitTest,
                _ => throw new NotImplementedException()
            };
        }
    }
}
