using RuriLib.Models.Data.Resources.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RuriLib.Models.Data.Resources;

/// <summary>
/// Reads resource values by picking random lines from a file.
/// </summary>
public class RandomLinesFromFileResource : ConfigResource
{
    private readonly RandomLinesFromFileResourceOptions options;
    private readonly List<string> source;
    private readonly object sourceLocker = new();
    private readonly Random random = new();

    /// <summary>
    /// Creates a new random file-backed resource.
    /// </summary>
    /// <param name="options">The resource options.</param>
    public RandomLinesFromFileResource(RandomLinesFromFileResourceOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        this.options = options;

        var lines = File.ReadAllLines(options.Location);

        source = options.IgnoreEmptyLines
            ? lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToList()
            : lines.ToList();
    }

    /// <inheritdoc/>
    public override string TakeOne()
    {
        lock (sourceLocker)
        {
            return TakeOneUnsafe();
        }
    }

    private string TakeOneUnsafe()
    {
        if (source.Count == 0)
        {
            throw new Exception($"Resource '{options.Name}' has no more valid lines to take");
        }

        var line = source[random.Next(source.Count)];

        if (options.Unique)
        {
            source.Remove(line);
        }

        return line;
    }

    /// <inheritdoc/>
    public override List<string> Take(int amount)
    {
        lock (sourceLocker)
        {
            List<string> lines = [];

            for (var i = 0; i < amount; i++)
            {
                lines.Add(TakeOneUnsafe());
            }

            return lines;
        }
    }
}
