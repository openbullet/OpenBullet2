using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Core.Services;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs;
using RuriLib.Models.Proxies;
using RuriLib.Parallelization.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Shared
{
    public partial class ProxyCheckJobViewer : IDisposable
    {
        [Parameter] public ProxyCheckJob Job { get; set; }

        [Inject] private IModalService Modal { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }
        [Inject] private MemoryJobLogger Logger { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        private bool changingBots = false;
        private Timer uiRefreshTimer;

        protected override void OnInitialized() => AddEventHandlers();

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                var interval = Math.Max(50, OBSettingsService.Settings.GeneralSettings.JobUpdateInterval);
                uiRefreshTimer = new Timer(new TimerCallback(async _ => await InvokeAsync(StateHasChanged)),
                    null, interval, interval);
            }
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
                Logger.LogSuccess(Job.Id, $"{Loc["ProxyChecked"]} ({result.Host}:{result.Port}) {Loc["withPing"]} {result.Ping} ms {Loc["andCountry"]} {result.Country}");
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

        private void AddEventHandlers()
        {
            if (OBSettingsService.Settings.GeneralSettings.EnableJobLogging)
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
        {
            uiRefreshTimer?.Dispose();
            RemoveEventHandlers();
        }

        ~ProxyCheckJobViewer()
            => Dispose();
    }
}
