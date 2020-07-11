using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using Radzen.Blazor;
using RuriLib.Models.Hits;
using RuriLib.Models.Jobs;
using RuriLib.Models.Jobs.Threading;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared
{
    public partial class MultiRunJobViewer
    {
        [Inject] public IModalService Modal { get; set; }
        [Inject] public PersistentSettingsService PersistentSettings { get; set; }

        [Parameter] public MultiRunJob Job { get; set; }
        int refreshInterval = 1000;
        GenericLogger logger;
        bool changingBots = false;
        RadzenGrid<Hit> hitsGrid;
        Hit selectedHit;

        private string CaptureWidth
        {
            get
            {
                if (Job.Hits.Count == 0)
                    return "200px";

                var longest = Job.Hits
                    .Select(h => h.CapturedDataString.Length)
                    .OrderBy(l => l)
                    .Last();

                // The 0.82 value is referred to Consolas font-style
                // since 2048 units in height correspond to 1126 units in width,
                // and the 12 is referred to 12px in the css
                var totalWidth = (int)(longest * 12 * 0.82);

                if (totalWidth < 200)
                    return "200px";

                return $"{totalWidth}px";
            }
        }

        protected override void OnInitialized()
        {
            PeriodicRefresh(refreshInterval);
            TryHookLogger();
        }

        private void SelectHit(Hit hit)
        {
            selectedHit = hit;
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

                if (Job.Manager != null)
                    await Job.Manager.SetConcurrentTasks(newAmount);

                Job.Bots = newAmount;
                changingBots = false;
            }
        }

        private void LogResult(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            var botData = details.Result.BotData;
            var data = botData.Line.Data;

            var message = string.Format(Loc["LineCheckedMessage"], data, botData.STATUS);
            var color = botData.STATUS switch
            {
                "SUCCESS" => "yellowgreen",
                "FAIL" => "tomato",
                "BAN" => "plum",
                "RETRY" => "yellow",
                "ERROR" => "red",
                "NONE" => "skyblue",
                _ => "orange"
            };
            logger.Log(GenericLogger.LogKind.Info, message, color);
        }

        private void LogError(object sender, Exception ex)
        {
            logger.LogError($"{Loc["TaskManagerError"]} {ex.Message}");
        }

        private void LogTaskError(object sender, ErrorDetails<MultiRunInput> details)
        {
            var proxy = details.Item.BotData.Proxy;
            var data = details.Item.BotData.Line.Data;
            logger.LogError($"{Loc["TaskError"]} ({proxy})({data})! {details.Exception.Message}");
        }

        private void LogCompleted(object sender, EventArgs e)
        {
            logger.LogInfo(Loc["TaskManagerCompleted"]);
        }

        private void TryHookLogger()
        {
            if (Job.Manager != null && PersistentSettings.OpenBulletSettings.GeneralSettings.EnableJobLogging)
            {
                try { Job.Manager.OnResult -= LogResult; } catch { }
                try { Job.Manager.OnTaskError -= LogTaskError; } catch { }
                try { Job.Manager.OnError -= LogError; } catch { }
                try { Job.Manager.OnCompleted -= LogCompleted; } catch { }

                try
                {
                    Job.Manager.OnResult += LogResult;
                    Job.Manager.OnTaskError += LogTaskError;
                    Job.Manager.OnError += LogError;
                    Job.Manager.OnCompleted += LogCompleted;
                }
                catch { }
            }
        }

        private async Task Start()
        {
            try
            {
                await Job.Start();
                TryHookLogger();
                logger.LogInfo(Loc["StartedChecking"]);
                PeriodicRefresh(refreshInterval);
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
                logger.LogInfo(Loc["SoftStopMessage"]);
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
                logger.LogInfo(Loc["HardStopMessage"]);
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
                logger.LogInfo(Loc["PauseMessage"]);
                await Job.Pause();
                logger.LogInfo(Loc["TaskManagerPaused"]);
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
                logger.LogInfo(Loc["ResumeMessage"]);
                PeriodicRefresh(refreshInterval);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async void PeriodicRefresh(int interval)
        {
            while (Job.Status != TaskManagerStatus.Idle && Job.Status != TaskManagerStatus.Paused)
            {
                await InvokeAsync(StateHasChanged);
                await Task.Delay(Math.Max(50, interval));
            }

            // A final one to refresh the button status
            await InvokeAsync(StateHasChanged);
        }
    }
}
