using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using OpenBullet2.Auth;
using OpenBullet2.Core.Services;
using OpenBullet2.Shared.Forms;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Setup
    {
        [Inject] private NavigationManager Nav { get; set; }
        [Inject] private OpenBulletSettingsService OBSettingsService { get; set; }
        [Inject] private AuthenticationStateProvider Auth { get; set; }
        [Inject] private IModalService Modal { get; set; }
        
        private readonly int finalStep = 5;
        private int step = 0;
        private readonly AdminAccount admin = new();

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await js.InvokeVoidAsync("startRandomSetupEffect");
            }
        }

        private void ChangeLanguage()
        {
            var parameters = new ModalParameters();
            parameters.Add(nameof(CultureSelector.SaveSettings), false);
            Modal.Show<CultureSelector>(Loc["ChooseYourLanguage"], parameters);
        }

        private class AdminAccount
        {
            [Required]
            [StringLength(32, ErrorMessage = "The username must be between 1 and 32 characters", MinimumLength = 1)]
            public string Username { get; set; } = "admin";

            [Required]
            [StringLength(32, ErrorMessage = "The password must be between 8 and 32 characters.", MinimumLength = 8)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [Compare("Password")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        private async Task SetupAdminAccount()
        {
            OBSettingsService.Settings.SecuritySettings.RequireAdminLogin = true;
            OBSettingsService.Settings.SecuritySettings.AdminUsername = admin.Username;
            OBSettingsService.Settings.SecuritySettings.SetupAdminPassword(admin.Password);

            // Authenticate the admin (we don't care about its IP)
            await ((OBAuthenticationStateProvider)Auth).AuthenticateUser(admin.Username, admin.Password, IPAddress.Parse("127.0.0.1"));

            step++;
        }

        private async Task CompleteSetup()
        {
            await OBSettingsService.Save();
            Nav.NavigateTo("/");
        }
    }
}
