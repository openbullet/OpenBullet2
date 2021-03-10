using Blazor.FileReader;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using OpenBullet2.Entities;
using OpenBullet2.Helpers;
using OpenBullet2.Services;
using RuriLib.Extensions;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenBullet2.Shared.Forms
{
    public partial class WordlistAdd : IDisposable
    {
        [CascadingParameter] public BlazoredModalInstance BlazoredModal { get; set; }

        [Inject] private IFileReaderService FileReaderService { get; set; }
        [Inject] private RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] private PersistentSettingsService PersistentSettings { get; set; }

        private ElementReference inputTypeFileElement;
        private List<string> wordlistTypes;
        private string selectedWordlistType;
        private Dictionary<WordlistEntity, MemoryStream> wordlists;
        private long max;
        private long value;
        private decimal progress;
        private string baseDirectory = ".";
        private bool validUpload = false;

        private Node selectedNode = null;
        private List<Node> nodes = new() { };

        protected override async Task OnInitializedAsync()
        {
            wordlistTypes = RuriLibSettings.Environment.WordlistTypes.Select(w => w.Name).ToList();
            selectedWordlistType = wordlistTypes.First();

            baseDirectory = Directory.Exists("UserData") ? "UserData" : Directory.GetCurrentDirectory();
            await LoadTree(baseDirectory);
        }

        private async Task ProcessUploadedWordlist()
        {
            max = 0;
            value = 0;
            progress = 0;
            StateHasChanged();

            var files = (await FileReaderService.CreateReference(inputTypeFileElement).EnumerateFilesAsync()).ToList();

            if (files.Count == 0)
                return;

            max = files.Count;
            wordlists = new();

            foreach (var file in files)
            {
                var fileInfo = await file.ReadFileInfoAsync();

                var wordlist = new WordlistEntity()
                {
                    Name = Path.GetFileNameWithoutExtension(fileInfo.Name),
                    Type = selectedWordlistType
                };

                var memoryStream = new MemoryStream();

                wordlists.Add(wordlist, memoryStream);

                StateHasChanged();

                await using var fs = await file.OpenReadAsync();
                var buffer = new byte[20480];
                int count;

                while ((count = await fs.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    await Task.Delay(1);
                    await memoryStream.WriteAsync(buffer, 0, count);
                }

                value++;
                progress = (decimal)value / max * 100;

                StateHasChanged();
            }

            validUpload = true;
        }

        private async Task LoadTree(string baseDirectory)
        {
            if (!Directory.Exists(baseDirectory))
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["DirectoryNonExistant"]);
                return;
            }

            // If for example C: and not C:/ add the /
            if (Regex.Match(baseDirectory, "^[A-Za-z]:$").Success)
            {
                baseDirectory += '/';
            }

            if (!PersistentSettings.OpenBulletSettings.SecuritySettings.AllowSystemWideFileAccess &&
                !baseDirectory.IsSubPathOf(Directory.GetCurrentDirectory()))
            {
                await js.AlertError(Loc["Unauthorized"], Loc["SystemWideFileAccessDisabled"]);
                return;
            }

            var folders = Directory.GetDirectories(baseDirectory);
            var files = Directory.GetFiles(baseDirectory);
            nodes = folders.Concat(files).Select(e => new Node(e)).ToList();
            var paths = nodes.Select(n => n.Path).ToArray();
            StateHasChanged();
        }

        private async Task Upload()
        {
            if (!validUpload)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["UploadFileFirst"]);
                return;
            }

            foreach (var wordlist in wordlists)
            {
                wordlist.Value.Seek(0, SeekOrigin.Begin);
                var fileName = Path.Combine("UserData", "Wordlists", Guid.NewGuid().ToString() + ".txt").Replace('\\', '/');
                FileStream fs = new(fileName, FileMode.Create);
                wordlist.Value.CopyTo(fs);
                fs.Close();
                wordlist.Key.FileName = fileName;
                wordlist.Key.Total = File.ReadLines(fileName).Count();
            }

            BlazoredModal.Close(ModalResult.Ok(wordlists.Select(wl => wl.Key).ToList()));
        }

        private async Task SelectFile()
        {
            if (selectedNode == null || selectedNode.IsDirectory)
            {
                await js.AlertError(Loc["Uh-Oh"], Loc["SelectFileFirst"]);
                return;
            }

            BlazoredModal.Close(ModalResult.Ok(new List<WordlistEntity>() {
                new() {
                    FileName = selectedNode.Path,
                    Total = File.ReadLines(selectedNode.Path).Count()
                }
            }));
        }

        public void Dispose()
        {
            if (wordlists != null)
            {
                foreach (var memoryStream in wordlists.Select(wl => wl.Value))
                {
                    memoryStream.Close();
                }
            }
        }

        ~WordlistAdd()
            => Dispose();
    }

    public class Node
    {
        public string Path { get; }
        public bool IsDirectory => (File.GetAttributes(Path) & FileAttributes.Directory) == FileAttributes.Directory;
        public string Name => System.IO.Path.GetFileName(Path);

        public IEnumerable<Node> Children
            => children ?? Directory.GetFileSystemEntries(Path).Select(e => new Node(e)).ToArray();

        private readonly Node[] children = null;

        public Node(string path)
        {
            Path = path;
        }
    }
}
