using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpenBullet2.Helpers;
using OpenBullet2.Logging;
using OpenBullet2.Core.Repositories;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Logging;
using RuriLib.Models.Hits;
using RuriLib.Models.Jobs;
using RuriLib.Parallelization.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenBullet2.Core.Services;
using OpenBullet2.Core.Entities;
using Microsoft.EntityFrameworkCore;
using RuriLib.Models.Configs;

namespace OpenBullet2.Shared
{
    public partial class MultiRunJobViewer : IDisposable
    {
        [Parameter] public MultiRunJob Job { get; set; }

        [Inject] private IModalService Modal { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }
        [Inject] private MemoryJobLogger Logger { get; set; }
        [Inject] private IProxyGroupRepository ProxyGroups { get; set; }
        [Inject] private NavigationManager Nav { get; set; }

        private bool changingBots = false;
        private string hitsFilter = "SUCCESS";
        private List<Hit> selectedHits = new();
        private Hit lastSelectedHit;
        private Timer uiRefreshTimer;
        private List<ProxyGroupEntity> proxyGroups;
        private CancellationTokenSource startCTS;

        protected override async Task OnInitializedAsync()
        {
            AddEventHandlers();
            proxyGroups = await ProxyGroups.GetAll().ToListAsync();
        }

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

                try
                {
                    await Job.ChangeBots(newAmount);
                    Job.Bots = newAmount;
                }
                catch (Exception ex)
                {
                    await js.AlertException(ex);
                }
                finally
                {
                    changingBots = false;
                }
            }
        }

        private void ChangeOptions()
        {
            Nav.NavigateTo($"jobs/edit/{Job.Id}");
        }

        private void LogResult(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            var botData = details.Result.BotData;
            var data = botData.Line.Data;
            var proxy = botData.Proxy != null
                ? $"{botData.Proxy.Host}:{botData.Proxy.Port}"
                : string.Empty;

            var message = string.Format(Loc["LineCheckedMessage"], data, proxy, botData.STATUS);
            var color = botData.STATUS switch {
                "SUCCESS" => "yellowgreen",
                "FAIL" => "tomato",
                "BAN" => "plum",
                "RETRY" => "yellow",
                "ERROR" => "red",
                "NONE" => "skyblue",
                _ => "orange"
            };
            Logger.Log(Job.Id, message, LogKind.Custom, color);
        }

        private void PlaySoundOnHit(object sender, ResultDetails<MultiRunInput, CheckResult> details)
        {
            if (details.Result.BotData.STATUS == "SUCCESS" && OBSettingsService.Settings.CustomizationSettings.PlaySoundOnHit)
            {
                _ = js.InvokeVoidAsync("playHitSound");
            }
        }

        private void LogError(object sender, Exception ex)
        {
            Logger.LogError(Job.Id, $"{Loc["TaskManagerError"]} {ex.Message}");
        }

        private void LogTaskError(object sender, ErrorDetails<MultiRunInput> details)
        {
            var data = details.Item.BotData.Line.Data;
            var proxy = details.Item.BotData.Proxy != null
                ? $"{details.Item.BotData.Proxy.Host}:{details.Item.BotData.Proxy.Port}"
                : string.Empty;
            Logger.LogError(Job.Id, $"{Loc["TaskError"]} ({data})({proxy})! {details.Exception.Message}");
        }

        private void LogCompleted(object sender, EventArgs e)
        {
            Logger.LogInfo(Job.Id, Loc["TaskManagerCompleted"]);
        }

        private async Task Start()
        {
            try
            {
                startCTS = new CancellationTokenSource();
                await AskCustomInputs();

                Logger.LogInfo(Job.Id, Loc["StartedWaiting"]);
                await Job.Start(startCTS.Token);
                Logger.LogInfo(Job.Id, Loc["StartedChecking"]);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
            finally
            {
                startCTS?.Dispose();
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
            if (Job.Status is JobStatus.Starting or JobStatus.Waiting)
            {
                startCTS.Cancel();
                return;
            }

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

        private async Task AskCustomInputs()
        {
            Job.CustomInputsAnswers.Clear();
            foreach (var input in Job.Config.Settings.InputSettings.CustomInputs)
            {
                var parameters = new ModalParameters();
                parameters.Add(nameof(CustomInputQuestion.Question), input.Description);
                parameters.Add(nameof(CustomInputQuestion.DefaultAnswer), input.DefaultAnswer);

                var modal = Modal.Show<CustomInputQuestion>(Loc["CustomInput"], parameters);
                var result = await modal.Result;
                var answer = result.Cancelled ? input.DefaultAnswer : (string)result.Data;
                Job.CustomInputsAnswers[input.VariableName] = answer;
            }
        }

        private static string GetHitColor(Hit hit) => hit.Type switch {
            "SUCCESS" => "var(--fg-hit)",
            "NONE" => "var(--fg-tocheck)",
            _ => "var(--fg-custom)"
        };

        private void HitClicked(Hit hit, MouseEventArgs e)
        {
            // If we held down CTRL
            if (e.CtrlKey)
            {
                // If already selected, deselect
                if (selectedHits.Contains(hit))
                {
                    selectedHits.Remove(hit);
                }
                // Otherwise add to selected list
                else
                {
                    selectedHits.Add(hit);
                    lastSelectedHit = hit;
                }
            }
            // If we held down SHIFT
            else if (e.ShiftKey)
            {
                // If we never clicked anything, treat as normal click
                if (lastSelectedHit == null)
                {
                    lastSelectedHit = hit;
                    selectedHits.Clear();
                    selectedHits.Add(hit);
                }
                // Otherwise select the range from last selected hit
                else
                {
                    selectedHits.Clear();
                    var filteredHits = GetFilteredHits();
                    var lastIndex = filteredHits.IndexOf(lastSelectedHit);
                    var currentIndex = filteredHits.IndexOf(hit);
                    var rangeStartIndex = Math.Min(lastIndex, currentIndex);
                    var rangeEndIndex = Math.Max(lastIndex, currentIndex);

                    selectedHits.AddRange(filteredHits.Skip(rangeStartIndex).Take(rangeEndIndex - rangeStartIndex + 1));
                    lastSelectedHit = hit;
                }
            }
            else
            {
                lastSelectedHit = hit;
                selectedHits.Clear();
                selectedHits.Add(hit);
            }
        }

        private async Task CopyHitDataCapture()
        {
            if (selectedHits.Count == 0)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            var sb = new StringBuilder();
            selectedHits.ForEach(i => sb.AppendLine($"{i.Data.Data} | {i.CapturedDataString}"));

            try
            {
                await js.CopyToClipboard(sb.ToString());
            }
            catch
            {
                await js.AlertError(Loc["CopyToClipboardFailed"], Loc["CopyToClipboardFailedMessage"]);
            }
        }

        private async Task CopyHitData()
        {
            if (selectedHits.Count == 0)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            var sb = new StringBuilder();
            selectedHits.ForEach(i => sb.AppendLine(i.Data.Data));

            try
            {
                await js.CopyToClipboard(sb.ToString());
            }
            catch
            {
                await js.AlertError(Loc["CopyToClipboardFailed"], Loc["CopyToClipboardFailedMessage"]);
            }
        }

        private async Task SendToDebugger()
        {
            if (lastSelectedHit == null)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            VolatileSettings.DebuggerOptions.TestData = lastSelectedHit.Data.Data;
            VolatileSettings.DebuggerOptions.WordlistType = lastSelectedHit.Data.Type.Name;
            VolatileSettings.DebuggerOptions.UseProxy = lastSelectedHit.Proxy != null;

            if (lastSelectedHit.Proxy != null)
            {
                VolatileSettings.DebuggerOptions.ProxyType = lastSelectedHit.Proxy.Type;
                VolatileSettings.DebuggerOptions.TestProxy = lastSelectedHit.Proxy.ToString();
            }
        }

        private async Task ShowFullLog()
        {
            if (lastSelectedHit == null)
            {
                await ShowNoHitSelectedWarning();
                return;
            }

            if (lastSelectedHit.BotLogger == null)
            {
                var errorMessage = lastSelectedHit.Config.Mode == ConfigMode.DLL ? Loc["BotLogCompiledConfigError"] : Loc["BotLogDisabledError"];
                
                await js.AlertError(Loc["Disabled"], errorMessage);
                return;
            }

            var parameters = new ModalParameters();
            parameters.Add(nameof(BotLoggerViewerModal.BotLogger), lastSelectedHit.BotLogger);

            Modal.Show<BotLoggerViewerModal>(Loc["BotLog"], parameters);
        }

        private void OnHitsFilterChanged(string value)
        {
            hitsFilter = value;
            StateHasChanged();
        }

        private List<Hit> GetFilteredHits() => hitsFilter switch {
            "SUCCESS" => Job.Hits.Where(h => h.Type == "SUCCESS").ToList(),
            "NONE" => Job.Hits.Where(h => h.Type == "NONE").ToList(),
            "CUSTOM" => Job.Hits.Where(h => h.Type != "SUCCESS" && h.Type != "NONE").ToList(),
            _ => throw new NotImplementedException()
        };

        private async Task ShowNoHitSelectedWarning()
            => await js.AlertError(Loc["Uh-Oh"], Loc["NoHitSelectedWarning"]);

        private string GetProxyGroupName(int id)
            => id == -1 ? "All" : proxyGroups.FirstOrDefault(g => g.Id == id)?.Name;

        private void AddEventHandlers()
        {
            if (OBSettingsService.Settings.GeneralSettings.EnableJobLogging)
            {
                Job.OnResult += LogResult;
                Job.OnResult += PlaySoundOnHit;
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
                Job.OnResult -= PlaySoundOnHit;
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
    }
}