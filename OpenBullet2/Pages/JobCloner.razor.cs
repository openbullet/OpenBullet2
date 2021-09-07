using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using OpenBullet2.Auth;
using OpenBullet2.Core.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using System;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Pages
{
    public partial class JobCloner
    {
        [Parameter] public int JobId { get; set; }

        [Inject] private IJobRepository JobRepo { get; set; }
        [Inject] private IGuestRepository GuestRepo { get; set; }
        [Inject] private JobManagerService Manager { get; set; }
        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private JobFactoryService JobFactory { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }

        private JobType jobType;
        private JobEntity jobEntity;
        private JobOptions jobOptions;
        private JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        private int uid = -1;

        protected override async Task OnInitializedAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            jobEntity = await JobRepo.Get(JobId);
            var oldOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(jobEntity.JobOptions, settings).Options;
            jobOptions = JobOptionsFactory.CloneExistant(oldOptions);
            jobType = jobEntity.JobType;
        }

        private bool CanSeeJob(int ownerId)
            => uid == 0 || ownerId == uid;

        private async Task Clone()
        {
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
            // entity = await JobRepo.GetAll().Include(j => j.Owner).OrderByDescending(e => e.Id).FirstAsync();

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
    }
}
