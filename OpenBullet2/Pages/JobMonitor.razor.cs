using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using OpenBullet2.Core.Services;
using OpenBullet2.Shared.Forms;
using RuriLib.Functions.Crypto;
using RuriLib.Models.Jobs.Monitor;
using RuriLib.Models.Jobs.Monitor.Actions;
using RuriLib.Models.Jobs.Monitor.Triggers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobMonitor : System.IDisposable
    {
        [Inject] private JobMonitorService MonitorService { get; set; }
        [Inject] private IModalService Modal { get; set; }

        // TODO: The service should be in charge of periodically saving, NOT the UI!!
        private Timer timer;
        private readonly string fileName = "UserData/triggeredActions.json";
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings 
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented 
        };
        private byte[] lastSavedHash = System.Array.Empty<byte>();

        protected override void OnInitialized()
        {
            if (!MonitorService.Initialized)
            {
                RestoreTriggeredActions();
                MonitorService.Initialized = true;
            }

            if (timer == null)
                timer = new Timer(new TimerCallback(_ => SaveStateIfChanged()), null, 10000, 10000);
        }

        private void RestoreTriggeredActions()
        {
            if (!File.Exists(fileName))
                return;

            var json = File.ReadAllText(fileName);
            MonitorService.TriggeredActions = JsonConvert.DeserializeObject<TriggeredAction[]>(json, jsonSettings).ToList();
        }

        private void AddNew()
            => MonitorService.TriggeredActions.Add(new TriggeredAction());

        private void Remove(TriggeredAction triggeredAction)
            => MonitorService.TriggeredActions.Remove(triggeredAction);

        private void RemoveAll()
            => MonitorService.TriggeredActions.Clear();

        private void SaveStateIfChanged()
        {
            var json = JsonConvert.SerializeObject(MonitorService.TriggeredActions.ToArray(), jsonSettings);
            var hash = Crypto.MD5(Encoding.UTF8.GetBytes(json));
            
            if (hash != lastSavedHash)
            {
                try
                {
                    File.WriteAllText(fileName, json);
                    lastSavedHash = hash;
                }
                catch
                {
                    // File probably in use
                }
            }
        }

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

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
