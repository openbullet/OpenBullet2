using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using RuriLib.Threading.Models;
using System;
using System.Threading.Tasks;

namespace OpenBullet2.Shared
{
    public partial class ProxyCheckJobViewer : IDisposable
    {
        [Inject] public IModalService Modal { get; set; }
        [Inject] public PersistentSettingsService PersistentSettings { get; set; }
        [Inject] public MemoryJobLogger Logger { get; set; }
        [Inject] public NavigationManager Nav { get; set; }

        [Parameter] public ProxyCheckJob Job { get; set; }
        bool changingBots = false;

        protected override void OnInitialized()
        {
            StartPeriodicRefresh();
            AddEventHandlers();
        }

        private async Task ChangeBots()
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(BotsSelector.Bots), Job.Bots);

            var modal = Modal.Show<BotsSelector>(Loc["EditBotsAmount"], parameters);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                var newAmount = (int)result.Data;
                changingBots = true;

                await Job.ChangeBots(newAmount);

                Job.Bots = newAmount;
                changingBots = false;
            }
        }

        private void ChangeOptions()
        {
            Nav.NavigateTo($"jobs/edit/{Job.Id}");
        }

        private void LogResult(object sender, ResultDetails<ProxyCheckInput, Proxy> details)
        {
            var result = details.Result;

            if (result.WorkingStatus == ProxyWorkingStatus.Working)
                Logger.LogSuccess(Job.Id, $"{Loc["ProxyChecked"]} ({result.Host}:{result.Port}) {Loc["withPing"]} {result.Ping} {Loc["andCountry"]} {result.Country}");
            else
                Logger.LogWarning(Job.Id, $"{Loc["ProxyChecked"]} ({result.Host}:{result.Port}) {Loc["asNotWorking"]}");
        }

        private void LogError(object sender, Exception ex)
        {
            Logger.LogError(Job.Id, $"{Loc["TaskManagerError"]} {ex.Message}");
        }

        private void LogTaskError(object sender, ErrorDetails<ProxyCheckInput> details)
        {
            var proxy = details.Item.Proxy;
            Logger.LogError(Job.Id, $"{Loc["TaskError"]} ({proxy.Host}:{proxy.Port})! {details.Exception.Message}");
        }

        private void LogCompleted(object sender, EventArgs e)
        {
            Logger.LogInfo(Job.Id, Loc["CompletedMessage"]);
        }

        private async Task Start()
        {
            try
            {
                // Start the periodic refresh after a second (so that the job has time to set its status to Waiting)
                Task _ = Task.Run(async () => { await Task.Delay(1000); StartPeriodicRefresh(); });

                Logger.LogInfo(Job.Id, Loc["StartedWaiting"]);
                await Job.Start();
                Logger.LogInfo(Job.Id, Loc["StartedChecking"]);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task Stop()
        {
            try
            {
                Logger.LogInfo(Job.Id, Loc["SoftStopMessage"]);
                await Job.Stop();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task Abort()
        {
            try
            {
                Logger.LogInfo(Job.Id, Loc["HardStopMessage"]);
                await Job.Abort();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task Pause()
        {
            try
            {
                Logger.LogInfo(Job.Id, Loc["PauseMessage"]);
                await Job.Pause();
                Logger.LogInfo(Job.Id, Loc["TaskManagerPaused"]);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task Resume()
        {
            try
            {
                await Job.Resume();
                Logger.LogInfo(Job.Id, Loc["ResumeMessage"]);
                StartPeriodicRefresh();
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task SkipWait()
        {
            try
            {
                Job.SkipWait();
                Logger.LogInfo(Job.Id, Loc["SkippedWait"]);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async void StartPeriodicRefresh()
        {
            while (Job.Status != JobStatus.Idle && Job.Status != JobStatus.Paused)
            {
                await InvokeAsync(StateHasChanged);
                await Task.Delay(Math.Max(50, PersistentSettings.OpenBulletSettings.GeneralSettings.JobUpdateInterval));
            }

            // A final one to refresh the button status
            await InvokeAsync(StateHasChanged);
        }

        private void AddEventHandlers()
        {
            if (PersistentSettings.OpenBulletSettings.GeneralSettings.EnableJobLogging)
            {
                Job.OnResult += LogResult;
                Job.OnTaskError += LogTaskError;
                Job.OnError += LogError;
                Job.OnCompleted += LogCompleted;
            }
        }

        private void RemoveEventHandlers()
        {
            try
            {
                Job.OnResult -= LogResult;
                Job.OnTaskError -= LogTaskError;
                Job.OnError -= LogError;
                Job.OnCompleted -= LogCompleted;
            }
            catch
            {

            }
        }

        public void Dispose()
            => RemoveEventHandlers();

        ~ProxyCheckJobViewer()
            => Dispose();
    }
}
