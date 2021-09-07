using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using OpenBullet2.Auth;
using OpenBullet2.Helpers;
using OpenBullet2.Core.Models.Jobs;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;

namespace OpenBullet2.Pages
{
    public partial class JobManager : IDisposable
    {
        [Inject] private IJobRepository JobRepo { get; set; }
        [Inject] private JobManagerService Manager { get; set; }
        [Inject] private IModalService Modal { get; set; }
        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }
        [Inject] private IGuestRepository GuestRepo { get; set; }

        private readonly object removeLock = new();
        private int uid = -1;
        private Timer uiRefreshTimer;
        private Dictionary<int, string> guests = new();

        protected async override Task OnInitializedAsync()
        {
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            // If admin, also show the owner of each job
            if (uid == 0)
            {
                var entities = await GuestRepo.GetAll().ToListAsync();
                
                foreach (var entity in entities)
                {
                    guests[entity.Id] = entity.Username;
                }
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                var interval = Math.Max(50, OBSettingsService.Settings.GeneralSettings.JobManagerUpdateInterval);
                uiRefreshTimer = new Timer(new TimerCallback(async _ => await InvokeAsync(StateHasChanged)),
                    null, interval, interval);
            }
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
                Manager.RemoveJob(job);
            }
            finally
            {
                Monitor.Exit(removeLock);
            }
        }

        public async Task RemoveAll()
        {
            var notIdleJobs = Manager.Jobs.Where(j => CanSeeJob(j.Id) && j.Status != JobStatus.Idle);

            if (notIdleJobs.Any())
            {
                await js.AlertError($"{Loc["JobNotIdle"]} #{notIdleJobs.First().Id}", Loc["JobNotIdleWarning"]);
                return;
            }

            // If admin, just purge all
            if (uid == 0)
            {
                JobRepo.Purge();
                Manager.Clear();
            }
            else
            {
                var entities = await JobRepo.GetAll().Include(j => j.Owner)
                .Where(j => j.Owner.Id == uid).ToListAsync();

                await JobRepo.Delete(entities);

                foreach (var job in Manager.Jobs.Where(j => j.OwnerId == uid))
                {
                    Manager.RemoveJob(job);
                }
            }
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

        public void Clone(Job job) => Nav.NavigateTo($"jobs/clone/{job.Id}");

        public void Dispose() => uiRefreshTimer?.Dispose();
    }
}
