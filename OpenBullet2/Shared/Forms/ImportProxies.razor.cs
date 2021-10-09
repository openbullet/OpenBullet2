using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OpenBullet2.Helpers;
using RuriLib.Models.Proxies;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpenBullet2.Shared.Forms
{
    public partial class ImportProxies
    {
        [CascadingParameter] public BlazoredModalInstance BlazoredModal { get; set; }

        [Inject] public IModalService ModalService { get; set; }

        private string pasteContent = "";
        private string fileName = "";
        private string fileContent = "";
        private string url = "";
        private ProxyType defaultType = ProxyType.Http;
        private string defaultUsername = "";
        private string defaultPassword = "";
        private bool canImportFile = false;

        private void ImportFromPaste() => ReturnLines(pasteContent);
        private void ImportFromFile() => ReturnLines(fileContent);

        private async Task ProcessFile(InputFileChangeEventArgs e)
        {
            if (e.FileCount == 0)
                return;

            try
            {
                fileContent = "";

                // Maximum of 10 files per upload
                foreach(var file in e.GetMultipleFiles(10))
                {
                    // Maximum 10 MB file upload
                    using var reader = new StreamReader(file.OpenReadStream(10 * 1000 * 1024));
                    fileContent += await reader.ReadToEndAsync() + Environment.NewLine;
                }

                canImportFile = true;
                await InvokeAsync(StateHasChanged);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private async Task ImportFromUrl()
        {
            try
            {
                using var client = new HttpClient();
                using var request = new HttpRequestMessage();

                request.RequestUri = new Uri(url);
                request.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.116 Safari/537.36");

                using var response = await client.SendAsync(request);
                var text = await response.Content.ReadAsStringAsync();
                ReturnLines(text);
            }
            catch (Exception ex)
            {
                await js.AlertException(ex);
            }
        }

        private void ReturnLines(string text)
        {
            var lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var dto = new DTOs.ProxiesForImportDto
            {
                Lines = lines,
                DefaultType = defaultType,
                DefaultUsername = defaultUsername,
                DefaultPassword = defaultPassword
            };

            BlazoredModal.Close(ModalResult.Ok(dto));
        }
    }
}
