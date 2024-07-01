using OpenBullet2.Core.Models.Data;
using OpenBullet2.Web.Attributes;

namespace OpenBullet2.Web.Dtos.Job.MultiRun;

/// <summary>
/// Information about a data pool to take data lines from.
/// </summary>
public class DataPoolOptionsDto : PolyDto
{
}

/// <summary>
/// Reads data lines from a wordlist.
/// </summary>
[PolyType("wordlistDataPool")]
[MapsFrom(typeof(WordlistDataPoolOptions))]
[MapsTo(typeof(WordlistDataPoolOptions))]
public class WordlistDataPoolOptionsDto : DataPoolOptionsDto
{
    /// <summary>
    /// The ID of the Wordlist in the repository.
    /// </summary>
    public int WordlistId { get; set; } = -1;
}

/// <summary>
/// Reads data lines from a file.
/// </summary>
[PolyType("fileDataPool")]
[MapsFrom(typeof(FileDataPoolOptions))]
[MapsTo(typeof(FileDataPoolOptions))]
public class FileDataPoolOptionsDto : DataPoolOptionsDto
{
    /// <summary>
    /// The path to the file on disk.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The Wordlist Type.
    /// </summary>
    public string WordlistType { get; set; } = "Default";
}

/// <summary>
/// Generates data lines from a range of values.
/// </summary>
[PolyType("rangeDataPool")]
[MapsFrom(typeof(RangeDataPoolOptions))]
[MapsTo(typeof(RangeDataPoolOptions))]
public class RangeDataPoolOptionsDto : DataPoolOptionsDto
{
    /// <summary>
    /// The start of the range.
    /// </summary>
    public long Start { get; set; } = 0;

    /// <summary>
    /// The length of the range.
    /// </summary>
    public int Amount { get; set; } = 100;

    /// <summary>
    /// The entity of the interval between elements.
    /// </summary>
    public int Step { get; set; } = 1;

    /// <summary>
    /// Whether to pad numbers with zeroes basing on the number
    /// of digits of the biggest number to generate.
    /// </summary>
    public bool Pad { get; set; } = false;

    /// <summary>
    /// The Wordlist Type.
    /// </summary>
    public string WordlistType { get; set; } = "Default";
}

/// <summary>
/// Generates data lines from combinations.
/// </summary>
[PolyType("combinationsDataPool")]
[MapsFrom(typeof(CombinationsDataPoolOptions))]
[MapsTo(typeof(CombinationsDataPoolOptions))]
public class CombinationsDataPoolOptionsDto : DataPoolOptionsDto
{
    /// <summary>
    /// The possible characters that can be in a combination, one after the other without separators.
    /// </summary>
    public string CharSet { get; set; } = "0123456789";

    /// <summary>
    /// The length of the combinations to generate.
    /// </summary>
    public int Length { get; set; } = 4;

    /// <summary>
    /// The Wordlist Type.
    /// </summary>
    public string WordlistType { get; set; } = "Default";
}

/// <summary>
/// Generates infinite blank data lines.
/// </summary>
[PolyType("infiniteDataPool")]
[MapsFrom(typeof(InfiniteDataPoolOptions))]
[MapsTo(typeof(InfiniteDataPoolOptions))]
public class InfiniteDataPoolOptionsDto : DataPoolOptionsDto
{
}
