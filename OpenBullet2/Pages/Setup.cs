using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpenBullet2.Services;
using OpenBullet2.Shared.Forms;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Setup
    {
        [Inject] public NavigationManager Nav { get; set; }
        [Inject] public PersistentSettingsService Settings { get; set; }
        [Inject] public IModalService Modal { get; set; }
        private readonly int finalStep = 4;
        private int step = 0;
        private readonly AdminAccount admin = new();

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await js.InvokeVoidAsync("startRainbowLines");
            }
        }

        private void ChangeLanguage()
        {
            Modal.Show<CultureSelector>(Loc["ChooseYourLanguage"]);
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

        private void SetupAdminAccount()
        {
            Settings.OpenBulletSettings.SecuritySettings.AdminUsername = admin.Username;
            Settings.OpenBulletSettings.SecuritySettings.SetupAdminPassword(admin.Password);
            step++;
        }

        private async Task CompleteSetup()
        {
            await Settings.Save();
            Nav.NavigateTo("/");
        }
    }
}
