using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Data.DataPools;

/// <summary>
/// Generates a numeric range as a data pool.
/// </summary>
public class RangeDataPool : DataPool
{
    /// <summary>
    /// The starting value of the range.
    /// </summary>
    public long Start { get; }

    /// <summary>
    /// The number of items to generate.
    /// </summary>
    public int Amount { get; }

    /// <summary>
    /// The increment applied between successive items.
    /// </summary>
    public int Step { get; }

    /// <summary>
    /// Whether the generated numbers should be zero-padded.
    /// </summary>
    public bool Pad { get; }

    /// <summary>
    /// The legacy pool code used when serializing this pool type.
    /// </summary>
    public readonly int POOL_CODE = -3;

    /// <summary>
    /// Creates a DataPool by counting numbers from <paramref name="start"/>, increasing
    /// by <paramref name="step"/> for <paramref name="amount"/> times.
    /// </summary>
    /// <param name="start">The first value in the range.</param>
    /// <param name="amount">The amount of values to generate.</param>
    /// <param name="step">The increment applied to each next value.</param>
    /// <param name="pad">Optionally adds an automatic padding basing on the longest
    /// number's amount of digits.</param>
    /// <param name="wordlistType">The associated wordlist type name.</param>
    /// <example>new DataPool(1, 10, 1, true) will give [01, 02.. 10]</example>
    public RangeDataPool(long start, int amount, int step = 1, bool pad = false, string wordlistType = "Default")
    {
        Start = start;
        Amount = amount;
        Step = step;
        Pad = pad;

        var end = start + step * (amount - 1);
        var maxLength = end.ToString().Length;
        DataList = Range(start, end, step)
            .Select(i => pad ? i.ToString().PadLeft(maxLength, '0') : i.ToString());
        Size = amount;
        WordlistType = wordlistType;
    }

    private static IEnumerable<long> Range(long min, long max, int step)
    {
        for (var i = min; i <= max; i += step)
        {
            yield return i;
        }
    }

    /// <inheritdoc/>
    public override void Reload()
    {
        var end = Start + Step * (Amount - 1);
        var maxLength = end.ToString().Length;
        DataList = Range(Start, end, Step)
            .Select(i => Pad ? i.ToString().PadLeft(maxLength, '0') : i.ToString());
    }
}
