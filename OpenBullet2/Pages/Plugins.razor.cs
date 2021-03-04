using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OpenBullet2.Helpers;
using RuriLib.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenBullet2.Pages
{
    public partial class Plugins
    {
        [Inject] private PluginRepository PluginRepo { get; set; }

        private async Task ProcessUploadedPlugins(InputFileChangeEventArgs e)
        {
            if (e.FileCount == 0)
                return;

            try
            {
                // Support maximum 5000 files at a time
                foreach (var file in e.GetMultipleFiles(5000))
                {
                    // Support maximum 5 MB per file
                    var stream = file.OpenReadStream(5 * 1000 * 1000);

                    // Copy the content to a MemoryStream
                    using var reader = new StreamReader(stream);
                    using var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);

                    // Add it to the repo
                    PluginRepo.AddPlugin(ms);
                }

                StateHasChanged();
                await js.AlertSuccess(Loc["AllDone"], $"{Loc["PluginsSuccessfullyUploaded"]}: {e.FileCount}");
            }
            catch (Exception ex)
            {
                await js.AlertError(ex.GetType().Name, ex.Message);
            }

            uploadDisplay = false;
        }
    }
}
