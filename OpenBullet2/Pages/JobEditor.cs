using Microsoft.AspNetCore.Components;
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
    public partial class JobEditor
    {
        [Inject] IJobRepository JobRepo { get; set; }
        [Inject] IHitRepository HitRepo { get; set; }
        [Inject] IWordlistRepository WordlistRepo { get; set; }
        [Inject] IProxyGroupRepository ProxyGroupRepo { get; set; }
        [Inject] IProxyRepository ProxyRepo { get; set; }
        [Inject] JobManagerService Manager { get; set; }
        [Inject] ConfigService ConfigService { get; set; }
        [Inject] RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] NavigationManager Nav { get; set; }

        [Parameter] public int JobId { get; set; }
        JobEntity jobEntity;
        JobOptions jobOptions;
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

        protected override async Task OnInitializedAsync()
        {
            jobEntity = await JobRepo.Get(JobId);
            jobOptions = JsonConvert.DeserializeObject<JobOptionsWrapper>(jobEntity.JobOptions, settings).Options;
        }

        private async Task Edit()
        {
            var wrapper = new JobOptionsWrapper { Options = jobOptions };
            jobEntity.JobOptions = JsonConvert.SerializeObject(wrapper, settings);
            
            await JobRepo.Update(jobEntity);

            var factory = new JobFactory(ConfigService, RuriLibSettings, HitRepo, ProxyRepo, ProxyGroupRepo, WordlistRepo);

            try
            {
                var oldJob = Manager.Jobs.First(j => j.Id == JobId);
                var newJob = factory.FromOptions(JobId, jobOptions);

                Manager.Jobs.Remove(oldJob);
                Manager.Jobs.Add(newJob);
                Nav.NavigateTo($"job/{JobId}");
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }
    }
}
