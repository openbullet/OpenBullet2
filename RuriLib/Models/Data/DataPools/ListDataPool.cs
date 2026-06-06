using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Data.DataPools;

/// <summary>
/// Exposes an existing sequence as a data pool.
/// </summary>
public class ListDataPool : DataPool
{
    /// <summary>
    /// Creates a DataPool given an IEnumerable <paramref name="list"/> and counts the amount of lines.
    /// </summary>
    /// <param name="list">The lines to expose.</param>
    /// <param name="wordlistType">The associated wordlist type name.</param>
    public ListDataPool(IEnumerable<string> list, string wordlistType = "Default")
    {
        ArgumentNullException.ThrowIfNull(list);

        DataList = list;
        Size = DataList.Count();
        WordlistType = wordlistType;
    }
}
