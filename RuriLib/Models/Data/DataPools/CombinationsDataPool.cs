using System;
using System.Linq;

namespace RuriLib.Models.Data.DataPools;

/// <summary>
/// Generates all fixed-length combinations from a character set.
/// </summary>
public class CombinationsDataPool : DataPool
{
    /// <summary>
    /// The character set used to generate combinations.
    /// </summary>
    public string CharSet { get; }

    /// <summary>
    /// The length of each generated combination.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// The legacy pool code used when serializing this pool type.
    /// </summary>
    public readonly int POOL_CODE = -4;

    /// <summary>
    /// Creates a DataPool by generating all the possible combinations of a string.
    /// </summary>
    /// <param name="charSet">The allowed character set (one after the other like in the string "abcdef")</param>
    /// <param name="length">The length of the output combinations</param>
    /// <param name="wordlistType">The associated wordlist type name.</param>
    public CombinationsDataPool(string charSet, int length, string wordlistType = "Default")
    {
        ArgumentNullException.ThrowIfNull(charSet);

        CharSet = charSet;
        Length = length;

        DataList = charSet.Select(x => x.ToString());
        for (var i = 0; i < length - 1; i++)
        {
            DataList = DataList.SelectMany(x => charSet, (x, y) => x + y);
        }

        var sizeDouble = Math.Pow(charSet.Length, length);
        Size = sizeDouble < long.MaxValue ? (long)sizeDouble : long.MaxValue;
        WordlistType = wordlistType;
    }

    /// <inheritdoc/>
    public override void Reload()
    {
        DataList = CharSet.Select(x => x.ToString());
        for (var i = 0; i < Length - 1; i++)
        {
            DataList = DataList.SelectMany(x => CharSet, (x, y) => x + y);
        }
    }
}
