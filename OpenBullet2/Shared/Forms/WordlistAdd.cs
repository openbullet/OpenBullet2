using Blazor.FileReader;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using OpenBullet2.Auth;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using Radzen;
using Radzen.Blazor;
using RuriLib.Extensions;
using RuriLib.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared.Forms
{
    public partial class WordlistAdd
    {
        [Inject] public IModalService ModalService { get; set; }
        [Inject] public IWordlistRepository WordlistRepo { get; set; }
        [Inject] public IGuestRepository GuestRepo { get; set; }
        [Inject] public IFileReaderService FileReaderService { get; set; }
        [Inject] public RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] public PersistentSettingsService PersistentSettings { get; set; }
        [Inject] public AuthenticationStateProvider Auth { get; set; }

        [CascadingParameter] public BlazoredModalInstance BlazoredModal { get; set; }
        ElementReference inputTypeFileElement;
        List<string> wordlistTypes;
        WordlistEntity wordlist;
        MemoryStream fileStream;
        long max;
        long value;
        decimal progress;
        string baseDirectory = ".";
        string selectedFile = "";
        IEnumerable<string> entries = null;
        bool valid = false;
        private int uid = -1;

        protected override async Task OnInitializedAsync()
        {
            wordlistTypes = RuriLibSettings.Environment.WordlistTypes.Select(w => w.Name).ToList();
            uid = await ((OBAuthenticationStateProvider)Auth).GetCurrentUserId();

            wordlist = new WordlistEntity
            {
                Name = "My Wordlist",
                Type = wordlistTypes.First(),
                Owner = await GuestRepo.Get(uid)
            };

            baseDirectory = Directory.GetCurrentDirectory();

            await LoadTree(baseDirectory);
        }

        private async Task ProcessUploadedWordlist()
        {
            max = 0;
            value = 0;
            StateHasChanged();
            var files = (await FileReaderService.CreateReference(inputTypeFileElement).EnumerateFilesAsync()).ToList();
            var file = files.FirstOrDefault();

            if (file == null)
                return;

            var fileInfo = await file.ReadFileInfoAsync();
            wordlist.Name = Path.GetFileNameWithoutExtension(fileInfo.Name);
            max = fileInfo.Size;
            StateHasChanged();

            using (var fs = await file.OpenReadAsync())
            {
                var buffer = new byte[20480];
                fileStream = new MemoryStream();
                int count;
                while ((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    value += count;
                    progress = ((decimal)fs.Position * 100) / fs.Length;
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(1);
                    await fileStream.WriteAsync(buffer, 0, count);
                }
            }
            StateHasChanged();
            valid = true;
        }

        async Task LoadTree(string baseDirectory)
        {
            if (!Directory.Exists(baseDirectory))
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["DirectoryNonExistant"]);
                return;
            }

            if (!PersistentSettings.OpenBulletSettings.SecuritySettings.AllowSystemWideFileAccess &&
                !baseDirectory.IsSubPathOf(Directory.GetCurrentDirectory()))
            {
                await js.AlertError(Loc["Unauthorized"], Loc["SystemWideFileAccessDisabled"]);
                return;
            }

            entries = Directory.GetDirectories(baseDirectory)
                               .Where(entry =>
                               {
                                   var name = Path.GetFileName(entry);

                                   return !name.StartsWith(".") && name != "bin" && name != "obj";
                               });
        }

        void LoadFiles(TreeExpandEventArgs args)
        {
            var directory = args.Value as string;

            args.Children.Data = Directory.EnumerateFileSystemEntries(directory);
            args.Children.Text = GetTextForNode;
            args.Children.HasChildren = (path) => Directory.Exists((string)path);
            args.Children.Template = FileOrFolderTemplate;
        }

        void LogChange(TreeEventArgs args)
        {
            selectedFile = args.Value as string;

            // Make sure it's a file
            if (File.Exists(selectedFile))
                valid = true;
        }

        string GetTextForNode(object data)
        {
            return Path.GetFileName((string)data);
        }

        RenderFragment<RadzenTreeItem> FileOrFolderTemplate = (context) => builder =>
        {
            string path = context.Value as string;
            bool isDirectory = Directory.Exists(path);

            builder.OpenComponent<RadzenIcon>(0);
            builder.AddAttribute(1, "Icon", isDirectory ? "folder" : "insert_drive_file");
            if (!isDirectory)
            {
                builder.AddAttribute(2, "Style", "margin-left: 24px");
            }
            builder.CloseComponent();
            builder.AddContent(3, context.Text);
        };

        private async Task Upload()
        {
            if (!valid)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["UploadFileFirst"]);
                return;
            }

            await WordlistRepo.Add(wordlist, fileStream);
            BlazoredModal.Close(ModalResult.Ok(wordlist));
        }

        private async Task SelectFile()
        {
            if (!valid)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["SelectFileFirst"]);
                return;
            }

            wordlist.FileName = selectedFile;
            wordlist.Total = File.ReadLines(selectedFile).Count();
            await WordlistRepo.Add(wordlist);
            BlazoredModal.Close(ModalResult.Ok(wordlist));
        }
    }
}
