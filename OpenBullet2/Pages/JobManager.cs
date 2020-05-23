using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobManager
    {
        [Inject] IJobRepository JobRepo { get; set; }
        [Inject] JobManagerService Manager { get; set; }
        
        [Inject] IModalService Modal { get; set; }
        [Inject] NavigationManager Nav { get; set; }

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
