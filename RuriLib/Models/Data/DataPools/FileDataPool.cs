using System;
using System.IO;
using System.Linq;

namespace RuriLib.Models.Data.DataPools;

/// <summary>
/// Loads data from a file on disk.
/// </summary>
public class FileDataPool : DataPool
{
    /// <summary>
    /// The path of the backing file.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// The legacy pool code used when serializing this pool type.
    /// </summary>
    public readonly int POOL_CODE = -2;

    /// <summary>
    /// Creates a DataPool by loading lines from a file with the given <paramref name="fileName"/>.
    /// </summary>
    /// <param name="fileName">The path of the file to read.</param>
    /// <param name="wordlistType">The associated wordlist type name.</param>
    public FileDataPool(string fileName, string wordlistType = "Default")
    {
        ArgumentNullException.ThrowIfNull(fileName);

        FileName = fileName;
        DataList = File.ReadLines(fileName);
        Size = DataList.Count();
        WordlistType = wordlistType;
    }

    /// <inheritdoc/>
    public override void Reload()
    {
        DataList = File.ReadLines(FileName);
    }
}
