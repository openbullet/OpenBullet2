using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs;
using RuriLib.Services;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobManager
    {
        [Inject] IJobRepository JobRepo { get; set; }
        [Inject] JobManagerService Manager { get; set; }
        [Inject] ConfigService ConfigService { get; set; }
        [Inject] RuriLibSettingsService SettingsService { get; set; }
        [Inject] IHitRepository HitRepo { get; set; }
        [Inject] IWordlistRepository WordlistRepo { get; set; }
        [Inject] IProxyGroupRepository ProxyGroupRepo { get; set; }
        [Inject] IProxyRepository ProxyRepo { get; set; }
        
        [Inject] IModalService Modal { get; set; }
        [Inject] NavigationManager Nav { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!Manager.Initialized)
            {
                await RestoreJobs();
                Manager.Initialized = true;
            }
        }

        private async Task RestoreJobs()
        {
            var entries = await JobRepo.GetAll().ToListAsync();
            var factory = new JobFactory(ConfigService, SettingsService, HitRepo, ProxyRepo, ProxyGroupRepo, WordlistRepo);
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var entry in entries)
            {
                var options = JsonConvert.DeserializeObject<JobOptionsWrapper>(entry.JobOptions, jsonSettings).Options;
                var job = factory.FromOptions(entry.Id, options);
                Manager.Jobs.Add(job);
            }
        }

        private void SelectJob(Job job)
        {
            Nav.NavigateTo($"job/{job.Id}");
        }

        private async Task NewJob()
        {
            var modal = Modal.Show<JobTypeSelector>("Select Job Type");
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var type = result.Data as JobType?;


                Nav.NavigateTo($"jobs/create/{type}");
            }
        }

        public async Task Remove(Job job)
        {
            var entity = await JobRepo.GetAll().FirstAsync(e => e.Id == job.Id);
            await JobRepo.Delete(entity);
            Manager.Jobs.Remove(job);
        }

        public Task RemoveAll()
        {
            JobRepo.Purge();
            Manager.Jobs.Clear();

            return Task.CompletedTask;
        }
    }
}
