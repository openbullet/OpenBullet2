using System;
using System.IO;
using System.Linq;

namespace RuriLib.Models.Data.DataPools;

/// <summary>
/// Loads data from a persisted <see cref="Wordlist"/>.
/// </summary>
public class WordlistDataPool : DataPool
{
    /// <summary>
    /// The wordlist used as the backing source.
    /// </summary>
    public Wordlist Wordlist { get; }

    /// <summary>
    /// Creates a DataPool by loading lines from a given <paramref name="wordlist"/>.
    /// </summary>
    /// <param name="wordlist">The wordlist to expose.</param>
    public WordlistDataPool(Wordlist wordlist)
    {
        ArgumentNullException.ThrowIfNull(wordlist);

        Wordlist = wordlist;
        DataList = File.ReadLines(wordlist.Path ?? throw new ArgumentException("The wordlist must reside on disk", nameof(wordlist)));
        Size = wordlist.Total;
        WordlistType = wordlist.Type.Name;
    }

    /// <inheritdoc/>
    public override void Reload()
    {
        DataList = File.ReadLines(Wordlist.Path ?? throw new InvalidOperationException("The wordlist must reside on disk"));
    }
}
