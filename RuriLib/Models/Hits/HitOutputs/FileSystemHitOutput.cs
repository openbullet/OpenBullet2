using RuriLib.Extensions;
using RuriLib.Functions.Files;
using System.IO;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs
{
    public class FileSystemHitOutput : IHitOutput
    {
        public string BaseDir { get; set; }

        public FileSystemHitOutput(string baseDir = "Hits")
        {
            BaseDir = baseDir;
        }

        public Task Store(Hit hit)
        {
            Directory.CreateDirectory(BaseDir);

            var folderName = Path.Combine(BaseDir, hit.Config.Metadata.Name.ToValidFileName());
            Directory.CreateDirectory(folderName);
            
            var fileName = Path.Combine(folderName, $"{hit.Type.ToValidFileName()}.txt");

            lock (FileLocker.GetHandle(fileName))
            {
                File.AppendAllTextAsync(fileName, $"{hit}\r\n");
            }

            return Task.CompletedTask;
        }
    }
}
