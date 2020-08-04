using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Entities;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
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
    public partial class JobMonitor
    {
        [Inject] JobMonitorService MonitorService { get; set; }
        [Inject] IModalService Modal { get; set; }

        private Timer timer;
        private readonly string fileName = "triggeredActions.json";
        private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings 
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented 
        };
        private byte[] lastSavedHash = new byte[0];

        protected override async Task OnInitializedAsync()
        {
            if (!MonitorService.Initialized)
            {
                await RestoreTriggeredActions();
                MonitorService.Initialized = true;
            }

            timer = new Timer(new TimerCallback(async _ => await SaveStateIfChanged()), null, 10000, 10000);
        }

        private async Task RestoreTriggeredActions()
        {
            if (!File.Exists(fileName))
                return;

            var json = await File.ReadAllTextAsync(fileName);
            MonitorService.TriggeredActions = JsonConvert.DeserializeObject<TriggeredAction[]>(json, jsonSettings).ToList();
        }

        private void AddNew()
            => MonitorService.TriggeredActions.Add(new TriggeredAction());

        private void Remove(TriggeredAction triggeredAction)
            => MonitorService.TriggeredActions.Remove(triggeredAction);

        private void RemoveAll()
            => MonitorService.TriggeredActions.Clear();

        private async Task SaveStateIfChanged()
        {
            var json = JsonConvert.SerializeObject(MonitorService.TriggeredActions.ToArray(), jsonSettings);
            var hash = Crypto.MD5(Encoding.UTF8.GetBytes(json));
            
            if (hash != lastSavedHash)
            {
                await File.WriteAllTextAsync(fileName, json);
                lastSavedHash = hash;
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

        private void RemoveAllActions(TriggeredAction ta)
            => ta.Actions.Clear();
    }
}
