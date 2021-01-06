using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Repositories
{
    public class DiskThemeRepository : IThemeRepository
    {
        private readonly string baseFolder;

        public DiskThemeRepository(string baseFolder)
        {
            this.baseFolder = baseFolder;

            Directory.CreateDirectory(baseFolder);
        }

        public async Task AddFromCss(string name, string css)
            => await File.WriteAllTextAsync($"UserData/Themes/{name}.css", css);

        public async Task AddFromCssFile(string fileName, Stream stream)
        {
            using var reader = new StreamReader(stream);
            using var fs = new FileStream($"UserData/Themes/{fileName}", FileMode.Create);
            await stream.CopyToAsync(fs);
        }

        public Task AddFromZipArchive(Stream stream)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

            // Make sure there's at least one .css file in the root of the archive
            var cssFiles = archive.Entries.Where(e => !e.FullName.Contains('/') && e.FullName.EndsWith(".css"));
            if (!cssFiles.Any())
                throw new FileNotFoundException("No css file found in the root of the provided archive!");

            archive.ExtractToDirectory(baseFolder);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<string>> GetNames()
            => Task.FromResult(Directory.GetFiles(baseFolder)
                    .Where(f => f.EndsWith(".css"))
                    .Select(f => Path.GetFileNameWithoutExtension(f)));


        public Task<string> GetPath(string name)
            => Task.FromResult(Path.Combine(baseFolder, $"{name}.css").Replace('\\', '/'));
    }
}
