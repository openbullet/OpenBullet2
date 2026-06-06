using System.Collections.Generic;

namespace RuriLib.Models.Data.DataPools;

/// <summary>
/// Exposes an infinite stream of empty strings.
/// </summary>
public class InfiniteDataPool : DataPool
{
    /// <summary>
    /// The legacy pool code used when serializing this pool type.
    /// </summary>
    public readonly int POOL_CODE = -5;

    /// <summary>
    /// Creates a DataPool of empty strings that never ends.
    /// </summary>
    /// <param name="wordlistType">The associated wordlist type name.</param>
    public InfiniteDataPool(string wordlistType = "Default")
    {
        DataList = InfiniteCounter();
        Size = int.MaxValue;
        WordlistType = wordlistType;
    }

    private static IEnumerable<string> InfiniteCounter()
    {
        while (true)
        {
            yield return string.Empty;
        }
    }

    /// <inheritdoc/>
    public override void Reload()
    {
        DataList = InfiniteCounter();
    }
}
