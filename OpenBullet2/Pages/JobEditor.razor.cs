using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;
using OpenBullet2.Auth;
using OpenBullet2.Core.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Pages
{
    public partial class JobEditor
    {
        [Parameter] public int JobId { get; set; }

        [Inject] private IJobRepository JobRepo { get; set; }
        [Inject] private JobManagerService Manager { get; set; }
        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private JobFactoryService JobFactory { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }

        private JobEntity jobEntity;
        private JobOptions jobOptions;
        private JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
        private int uid = -1;

        protected override async Task OnInitializedAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();
            jobEntity = await JobRepo.Get(JobId);
            jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(jobEntity.JobOptions, settings).Options;
        }

        private bool CanSeeJob(int ownerId)
            => uid == 0 || ownerId == uid;

        private async Task Edit()
        {
            var wrapper = new JobOptionsWrapper { Options = jobOptions };
            jobEntity.JobOptions = JsonConvert.SerializeObject(wrapper, settings);
            
            await JobRepo.Update(jobEntity);

            try
            {
                var oldJob = Manager.Jobs.First(j => j.Id == JobId);
                var newJob = JobFactory.FromOptions(JobId, jobEntity.Owner == null ? 0 : jobEntity.Owner.Id, jobOptions);

                Manager.RemoveJob(oldJob);
                Manager.AddJob(newJob);
                Nav.NavigateTo($"job/{JobId}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
