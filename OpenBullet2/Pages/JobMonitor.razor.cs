using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Core.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobMonitor
    {
        [Inject] private JobMonitorService MonitorService { get; set; }
        [Inject] private IModalService Modal { get; set; }

        private void AddNew()
            => MonitorService.TriggeredActions.Add(new TriggeredAction());

        private void Remove(TriggeredAction triggeredAction)
            => MonitorService.TriggeredActions.Remove(triggeredAction);

        private void RemoveAll()
            => MonitorService.TriggeredActions.Clear();

        private async Task AddNewTrigger(TriggeredAction ta)
        {
            var modal = Modal.Show<TriggerTypeSelector>(Loc["SelectTriggerType"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                ta.Triggers.Add(result.Data as Trigger);
            }
        }

        private async Task EditTrigger(Trigger t)
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(TriggerEdit.Trigger), t);

            var modal = Modal.Show<TriggerEdit>(Loc["EditTrigger"], parameters);
            await modal.Result;
            StateHasChanged();
        }

        private void RemoveAllTriggers(TriggeredAction ta)
            => ta.Triggers.Clear();

        private async Task AddNewAction(TriggeredAction ta)
        {
            var modal = Modal.Show<ActionTypeSelector>(Loc["SelectActionType"]);
            var result = await modal.Result;

            if (!result.Cancelled)
            {
                ta.Actions.Add(result.Data as Action);
            }
        }

        private async Task EditAction(Action a)
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(ActionEdit.Action), a);

            var modal = Modal.Show<ActionEdit>(Loc["EditAction"], parameters);
            await modal.Result;
            StateHasChanged();
        }

        private void RemoveAllActions(TriggeredAction ta)
            => ta.Actions.Clear();
    }
}
