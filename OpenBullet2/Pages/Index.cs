using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Index
    {
        [Inject] public MetricsService Metrics { get; set; }
        [Inject] public AuthenticationStateProvider Auth { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        [Inject] public IModalService Modal { get; set; }

        protected override void OnInitialized()
        {
            StartPeriodicRefresh();
        }

        private void StartPeriodicRefresh()
        {
            var timer = new Timer(new TimerCallback(async _ =>
            {
                await InvokeAsync(StateHasChanged);
            }), null, 1000, 1000);

            var cpuTimer = new Timer(new TimerCallback(async _ =>
            {
                await Metrics.UpdateCpuUsage();
            }), null, 500, 500);
        }

        private string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + $" {suf[0]}";
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString("0.##", CultureInfo.InvariantCulture) + $" {suf[place]}";
        }

        private async Task Logout()
        {
            await ((Auth.OBAuthenticationStateProvider)Auth).Logout();
            Nav.NavigateTo("/");
        }
    }
}
