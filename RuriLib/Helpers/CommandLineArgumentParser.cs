using System;
using System.Collections.Generic;
using System.Text;

namespace RuriLib.Helpers;

/// <summary>
/// Splits browser command-line arguments while preserving quoted values.
/// </summary>
public static class CommandLineArgumentParser
{
    /// <summary>
    /// Splits one or more argument segments into individual command-line arguments.
    /// </summary>
    public static string[] ParseMany(params string?[] argumentSegments)
    {
        var arguments = new List<string>();

        foreach (var segment in argumentSegments)
        {
            arguments.AddRange(Parse(segment));
        }

        return [.. arguments];
    }

    /// <summary>
    /// Splits a single argument segment into individual command-line arguments.
    /// </summary>
    public static string[] Parse(string? argumentSegment)
    {
        if (string.IsNullOrWhiteSpace(argumentSegment))
        {
            return [];
        }

        var arguments = new List<string>();
        var current = new StringBuilder();
        char? activeQuote = null;
        var tokenStarted = false;

        for (var i = 0; i < argumentSegment.Length; i++)
        {
            var c = argumentSegment[i];

            if (activeQuote is not null)
            {
                if (c == activeQuote)
                {
                    activeQuote = null;
                    continue;
                }

                if (c == '\\' && i + 1 < argumentSegment.Length)
                {
                    var next = argumentSegment[i + 1];
                    if (next == activeQuote || next == '\\')
                    {
                        current.Append(next);
                        tokenStarted = true;
                        i++;
                        continue;
                    }
                }

                current.Append(c);
                tokenStarted = true;
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (!tokenStarted)
                {
                    continue;
                }

                arguments.Add(current.ToString());
                current.Clear();
                tokenStarted = false;
                continue;
            }

            if (c is '"' or '\'')
            {
                activeQuote = c;
                tokenStarted = true;
                continue;
            }

            current.Append(c);
            tokenStarted = true;
        }

        if (tokenStarted)
        {
            arguments.Add(current.ToString());
        }

        return [.. arguments];
    }
}
