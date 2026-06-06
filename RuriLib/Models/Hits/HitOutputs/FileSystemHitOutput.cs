using RuriLib.Extensions;
using RuriLib.Models.Data.DataPools;
using RuriLib.Functions.Files;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RuriLib.Models.Hits.HitOutputs;

/// <summary>
/// Stores hits on the local filesystem.
/// </summary>
public class FileSystemHitOutput : IHitOutput
{
    private const string ConfigPlaceholder = "<CONFIG>";
    private const string WordlistPlaceholder = "<WORDLIST>";
    private const string DatePlaceholder = "<DATE>";

    /// <summary>Gets or sets the base output directory.</summary>
    public string BaseDir { get; set; }

    /// <summary>
    /// Creates a filesystem hit output.
    /// </summary>
    /// <param name="baseDir">The base output directory.</param>
    public FileSystemHitOutput(string baseDir = "Hits")
    {
        BaseDir = baseDir;
    }

    /// <inheritdoc />
    public Task Store(Hit hit)
    {
        var folderName = ResolveFolderName(hit);
        Directory.CreateDirectory(folderName);

        var fileName = Path.Combine(folderName, $"{hit.Type.ToValidFileName()}.txt");

        lock (FileLocker.GetHandle(fileName))
        {
            File.AppendAllText(fileName, $"{hit}{System.Environment.NewLine}");
        }

        return Task.CompletedTask;
    }

    private string ResolveFolderName(Hit hit)
    {
        if (!ContainsPlaceholders(BaseDir))
        {
            // Keep the historical behavior for existing jobs that only store a parent folder
            // such as "Hits", which previously resolved to "Hits/<CONFIG>/<TYPE>.txt".
            return Path.Combine(BaseDir, hit.Config.Metadata.Name.ToValidFileName());
        }

        return new StringBuilder(BaseDir)
            .Replace(ConfigPlaceholder, hit.Config.Metadata.Name.ToValidFileName())
            .Replace(WordlistPlaceholder, GetWordlistName(hit).ToValidFileName())
            .Replace(DatePlaceholder, hit.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            .ToString();
    }

    private static bool ContainsPlaceholders(string value)
        => value.Contains(ConfigPlaceholder)
            || value.Contains(WordlistPlaceholder)
            || value.Contains(DatePlaceholder);

    private static string GetWordlistName(Hit hit)
        => hit.DataPool switch
        {
            WordlistDataPool wordlistDataPool => wordlistDataPool.Wordlist.Name,
            FileDataPool fileDataPool => Path.GetFileNameWithoutExtension(fileDataPool.FileName),
            RangeDataPool rangeDataPool => $"{rangeDataPool.Start}|{rangeDataPool.Amount}|{rangeDataPool.Step}|{rangeDataPool.Pad}",
            CombinationsDataPool combinationsDataPool => $"{combinationsDataPool.Length}|{combinationsDataPool.CharSet}",
            InfiniteDataPool => string.Empty,
            _ => string.Empty
        };
}
