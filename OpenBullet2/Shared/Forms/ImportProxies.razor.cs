using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using OpenBullet2.Helpers;
using RuriLib.Models.Proxies;
using System;
using System.IO;
using System.Linq;
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

        private void ImportFromPaste()
        {
            ReturnLines(pasteContent);
        }

        private void ImportFromFile()
        {
            ReturnLines(fileContent);
        }

        private async Task ProcessFile(InputFileChangeEventArgs e)
        {
            if (e.FileCount == 0)
                return;

            try
            {
                // Maximum 10 MB file upload
                using var reader = new StreamReader(e.File.OpenReadStream(10 * 1000 * 1000));
                fileContent = await reader.ReadToEndAsync();
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
                var response = await client.GetAsync(url);
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
            string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var dto = new OpenBullet2.DTOs.ProxiesForImportDto
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
