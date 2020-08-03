using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using RuriLib.Models.Jobs.Monitor;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class JobMonitor
    {
        [Inject] ITriggeredActionRepository TriggeredActionRepo { get; set; }
        [Inject] JobMonitorService MonitorService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!MonitorService.Initialized)
            {
                await RestoreTriggeredActions();
                MonitorService.Initialized = true;
            }
        }

        private async Task RestoreTriggeredActions()
        {
            var entries = await TriggeredActionRepo.GetAll().ToListAsync();
            var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };

            foreach (var entry in entries)
            {
                var action = JsonConvert.DeserializeObject<TriggeredAction>(entry.TriggeredAction, jsonSettings);
                MonitorService.TriggeredActions.Add(action);
            }
        }
    }
}
