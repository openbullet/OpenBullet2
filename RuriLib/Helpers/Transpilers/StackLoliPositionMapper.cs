using RuriLib.Models.Blocks;
using System;
using System.Collections.Generic;

namespace RuriLib.Helpers.Transpilers;

/// <summary>
/// Maps editor positions between Stacker blocks and LoliCode lines.
/// </summary>
public static class StackLoliPositionMapper
{
    /// <summary>
    /// Gets the block index that best matches a line of LoliCode.
    /// </summary>
    /// <param name="script">The LoliCode script.</param>
    /// <param name="lineNumber">The 1-based line number.</param>
    /// <returns>The matching block index, if any.</returns>
    public static int? GetBlockIndexAtLine(string script, int lineNumber)
    {
        if (lineNumber < 1 || string.IsNullOrEmpty(script))
        {
            return null;
        }

        var lines = SplitLines(script);
        var currentBlockIndex = 0;
        var localLineNumber = 0;

        while (localLineNumber < lines.Length)
        {
            var startLine = localLineNumber + 1;
            var line = lines[localLineNumber];
            var trimmedLine = line.Trim();
            localLineNumber++;

            if (trimmedLine.StartsWith("BLOCK:", StringComparison.Ordinal))
            {
                while (localLineNumber < lines.Length)
                {
                    trimmedLine = lines[localLineNumber].Trim();
                    localLineNumber++;

                    if (trimmedLine.StartsWith("ENDBLOCK", StringComparison.Ordinal))
                    {
                        break;
                    }
                }

                var endLine = localLineNumber;

                if (lineNumber >= startLine && lineNumber <= endLine)
                {
                    return currentBlockIndex;
                }

                currentBlockIndex++;
                continue;
            }

            var endLineForRawSegment = startLine;
            var hasMeaningfulContent = !string.IsNullOrWhiteSpace(line);
            var lastMeaningfulLine = hasMeaningfulContent ? startLine : 0;

            while (localLineNumber < lines.Length)
            {
                trimmedLine = lines[localLineNumber].Trim();

                if (trimmedLine.StartsWith("BLOCK:", StringComparison.Ordinal))
                {
                    break;
                }

                if (!string.IsNullOrWhiteSpace(lines[localLineNumber]))
                {
                    hasMeaningfulContent = true;
                    lastMeaningfulLine = localLineNumber + 1;
                }

                localLineNumber++;
                endLineForRawSegment++;
            }

            if (lineNumber >= startLine && lineNumber <= endLineForRawSegment)
            {
                if (localLineNumber < lines.Length && lastMeaningfulLine > 0 && lineNumber > lastMeaningfulLine)
                {
                    return currentBlockIndex + 1;
                }

                if (hasMeaningfulContent)
                {
                    return currentBlockIndex;
                }

                if (localLineNumber < lines.Length)
                {
                    return currentBlockIndex;
                }

                return currentBlockIndex > 0 ? currentBlockIndex - 1 : null;
            }

            if (hasMeaningfulContent)
            {
                currentBlockIndex++;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the first LoliCode line generated for the block at the given index.
    /// </summary>
    /// <param name="blocks">The stack of blocks.</param>
    /// <param name="blockIndex">The 0-based block index.</param>
    /// <returns>The 1-based start line, if any.</returns>
    public static int? GetLineNumberForBlock(IReadOnlyList<BlockInstance> blocks, int blockIndex)
    {
        if (blockIndex < 0 || blockIndex >= blocks.Count)
        {
            return null;
        }

        var currentLine = 1;

        for (var i = 0; i < blocks.Count; i++)
        {
            if (i > 0 && blocks[i - 1] is not LoliCodeBlockInstance && blocks[i] is not LoliCodeBlockInstance)
            {
                currentLine = AdvanceLineAfterWrite(currentLine, Environment.NewLine);
            }

            if (i == blockIndex)
            {
                return currentLine;
            }

            currentLine = AdvanceLineAfterWrite(currentLine, GetSerializedBlockText(blocks[i]));
        }

        return null;
    }

    private static string GetSerializedBlockText(BlockInstance block)
    {
        if (block is LoliCodeBlockInstance)
        {
            var loliCode = block.ToLC();
            return EndsWithNewLine(loliCode) ? loliCode : $"{loliCode}{Environment.NewLine}";
        }

        return $"BLOCK:{block.Id}{Environment.NewLine}{block.ToLC()}ENDBLOCK{Environment.NewLine}";
    }

    private static int AdvanceLineAfterWrite(int currentLine, string text)
        => currentLine + CountLineBreaks(text);

    private static int CountLineBreaks(string text)
    {
        var lineBreaks = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r')
            {
                lineBreaks++;

                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }
            }
            else if (text[i] == '\n')
            {
                lineBreaks++;
            }
        }

        return lineBreaks;
    }

    private static bool EndsWithNewLine(string text)
        => text.EndsWith("\r\n", StringComparison.Ordinal) || text.EndsWith('\n');

    private static string[] SplitLines(string script)
        => script.Split(["\r\n", "\n"], StringSplitOptions.None);
}
