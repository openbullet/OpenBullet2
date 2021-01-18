using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Auth;
using OpenBullet2.Helpers;
using OpenBullet2.Models.Jobs;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobManager
    {
        [Inject] IJobRepository JobRepo { get; set; }
        [Inject] JobManagerService Manager { get; set; }
        [Inject] IModalService Modal { get; set; }
        [Inject] NavigationManager Nav { get; set; }
        [Inject] public PersistentSettingsService PersistentSettings { get; set; }
        [Inject] AuthenticationStateProvider Auth { get; set; }

        private readonly object removeLock = new object();
        private int uid = -1;

        protected override async Task OnInitializedAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            StartPeriodicRefresh();
        }

        private bool CanSeeJob(int ownerId)
            => uid == 0 || ownerId == uid;

        private void SelectJob(Job job)
        {
            Nav.NavigateTo($"job/{job.Id}");
        }

        private async Task NewJob()
        {
            var modal = Modal.Show<JobTypeSelector>(Loc["SelectJobType"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var type = result.Data as JobType?;


                Nav.NavigateTo($"jobs/create/{type}");
            }
        }

        public async Task Remove(Job job)
        {
            if (job.Status != JobStatus.Idle)
            {
                await js.AlertError(Loc["JobNotIdle"], Loc["JobNotIdleWarning"]);
                return;
            }

            while (!Monitor.TryEnter(removeLock))
                await Task.Delay(100);

            // If the user already deleted this job (e.g. clicked remove twice)
            if (!Manager.Jobs.Contains(job))
            {
                Monitor.Exit(removeLock);
                return;
            }

            try
            {
                var entity = await JobRepo.GetAll().FirstAsync(e => e.Id == job.Id);
                await JobRepo.Delete(entity);
                Manager.Jobs.Remove(job);
            }
            finally
            {
                Monitor.Exit(removeLock);
            }
        }

        public async Task RemoveAll()
        {
            var notIdleJobs = Manager.Jobs.Where(j => j.Status != JobStatus.Idle);

            if (notIdleJobs.Count() > 0)
            {
                await js.AlertError($"{Loc["JobNotIdle"]} #{notIdleJobs.First().Id}", Loc["JobNotIdleWarning"]);
                return;
            }

            JobRepo.Purge();
            Manager.Jobs.Clear();
        }

        public async Task Edit(Job job)
        {
            if (job.Status != JobStatus.Idle)
            {
                await js.AlertError(Loc["JobNotIdle"], Loc["JobNotIdleWarning"]);
                return;
            }

            Nav.NavigateTo($"jobs/edit/{job.Id}");
        }

        public void Clone(Job job)
        {
            Nav.NavigateTo($"jobs/clone/{job.Id}");
        }

        private async void StartPeriodicRefresh()
        {
            while (Manager.Jobs.Any(j => j.Status != JobStatus.Idle && j.Status != JobStatus.Paused))
            {
                await InvokeAsync(StateHasChanged);
                await Task.Delay(System.Math.Max(50, PersistentSettings.OpenBulletSettings.GeneralSettings.JobManagerUpdateInterval));
            }
        }
    }
}
