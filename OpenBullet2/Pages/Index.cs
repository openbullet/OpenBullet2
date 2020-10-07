using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using OpenBullet2.Services;
using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Index : IDisposable
    {
        [Inject] public MetricsService Metrics { get; set; }
        [Inject] public AuthenticationStateProvider Auth { get; set; }
        [Inject] public NavigationManager Nav { get; set; }
        [Inject] public IModalService Modal { get; set; }
        [Inject] public IHttpContextAccessor HttpAccessor { get; set; }
        public IPAddress IP { get; set; } = IPAddress.Parse("127.0.0.1");
        private Timer timer;
        private Timer cpuTimer;

        protected override void OnInitialized()
        {
            StartPeriodicRefresh();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
                IP = HttpAccessor.HttpContext.Connection?.RemoteIpAddress;
        }

        private void StartPeriodicRefresh()
        {
            timer = new Timer(new TimerCallback(async _ =>
            {
                await InvokeAsync(StateHasChanged);
            }), null, 1000, 1000);

            cpuTimer = new Timer(new TimerCallback(async _ =>
            {
                await Metrics.UpdateCpuUsage();
            }), null, 500, 500);
        }

        private async Task Logout()
        {
            await ((Auth.OBAuthenticationStateProvider)Auth).Logout();
            Nav.NavigateTo("/", true);
        }

        public void Dispose()
        {
            timer.Dispose();
            cpuTimer.Dispose();
        }
    }
}
