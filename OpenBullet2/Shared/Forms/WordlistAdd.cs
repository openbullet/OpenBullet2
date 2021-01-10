using Blazor.FileReader;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Repositories;
using OpenBullet2.Services;
using Radzen;
using Radzen.Blazor;
using RuriLib.Extensions;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared.Forms
{
    public partial class WordlistAdd : IDisposable
    {
        [Inject] public IModalService ModalService { get; set; }
        [Inject] public IFileReaderService FileReaderService { get; set; }
        [Inject] public RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] public PersistentSettingsService PersistentSettings { get; set; }
        [Inject] public IWordlistRepository WordlistRepo { get; set; }

        [CascadingParameter] public BlazoredModalInstance BlazoredModal { get; set; }
        ElementReference inputTypeFileElement;
        List<string> wordlistTypes;
        WordlistEntity wordlist;
        MemoryStream memoryStream;
        long max;
        long value;
        decimal progress;
        string baseDirectory = ".";
        string selectedFile = "";
        bool valid = false;

        Node selectedNode = null;
        List<Node> nodes = new() { };

        protected override async Task OnInitializedAsync()
        {
            wordlistTypes = RuriLibSettings.Environment.WordlistTypes.Select(w => w.Name).ToList();

            wordlist = new WordlistEntity
            {
                Name = "My Wordlist",
                Type = wordlistTypes.First()
            };

            baseDirectory = Directory.Exists("UserData") ? "UserData" : Directory.GetCurrentDirectory();
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
                memoryStream = new MemoryStream();
                int count;
                while ((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    value += count;
                    progress = ((decimal)fs.Position * 100) / fs.Length;
                    await InvokeAsync(StateHasChanged);
                    await Task.Delay(1);
                    await memoryStream.WriteAsync(buffer, 0, count);
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

            nodes = Directory.GetFileSystemEntries(baseDirectory).Select(e => new Node(e)).ToList();
            var paths = nodes.Select(n => n.Path).ToArray();
            StateHasChanged();
        }

        private async Task Upload()
        {
            if (!valid)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["UploadFileFirst"]);
                return;
            }

            memoryStream.Seek(0, SeekOrigin.Begin);
            var fileName = Path.Combine("UserData", "Wordlists", Guid.NewGuid().ToString() + ".txt").Replace('\\', '/');
            FileStream fs = new(fileName, FileMode.Create);
            memoryStream.CopyTo(fs);
            fs.Close();
            wordlist.FileName = fileName;
            wordlist.Total = File.ReadLines(fileName).Count();

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
            BlazoredModal.Close(ModalResult.Ok(wordlist));
        }

        public void Dispose()
            => memoryStream?.Close();

        ~WordlistAdd()
            => Dispose();
    }

    public class Node
    {
        public string Path { get; }
        public bool IsDirectory
        {
            get
            {
                try
                {
                    return (File.GetAttributes(Path) & FileAttributes.Directory) == FileAttributes.Directory;
                }
                catch
                {
                    return false;
                }
            }
        }
        public string Name => System.IO.Path.GetFileName(Path);
        private Node[] children = null;
        public IEnumerable<Node> Children
        {
            get
            {
                if (children == null)
                    children = Directory.GetFileSystemEntries(Path).Select(e => new Node(e)).ToArray();

                return children;
            }
        }

        public Node(string path)
        {
            Path = path;
        }
    }
}
