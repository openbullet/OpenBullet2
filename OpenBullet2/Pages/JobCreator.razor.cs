using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using OpenBullet2.Auth;
using OpenBullet2.Core.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using RuriLib.Models.Jobs;
using System;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Pages
{
    public partial class JobCreator
    {
        [Parameter] public string Type { get; set; }

        [Inject] private IJobRepository JobRepo { get; set; }
        [Inject] private IGuestRepository GuestRepo { get; set; }
        [Inject] private JobManagerService Manager { get; set; }
        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private JobFactoryService JobFactory { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }

        private JobType jobType;
        private JobOptions jobOptions;
        private int uid = -1;

        protected override async Task OnInitializedAsync()
        {
            uid = await((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            Type ??= JobType.MultiRun.ToString();

            var factory = new JobOptionsFactory();
            jobType = Enum.Parse<JobType>(Type);
            jobOptions = JobOptionsFactory.CreateNew(jobType);
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

                Manager.AddJob(job);
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
                PuppeteerUnitTestJob _ => JobType.PuppeteerUnitTest,
                _ => throw new NotImplementedException()
            };
        }
    }
}
