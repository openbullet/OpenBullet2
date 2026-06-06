using RuriLib.Legacy.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Legacy.LS;

/// <summary>
/// Parses a block from LoliScript code.
/// </summary>
public static class BlockParser
{
    /// <summary>
    /// The allowed block identifiers.
    /// </summary>
    public static Dictionary<string, Type> BlockMappings { get; set; } = new()
    {
        { "CAPTCHA", typeof(BlockImageCaptcha) }, // Obsolete
        { "RECAPTCHA", typeof(BlockRecaptcha) }, // Obsolete
        { "SOLVECAPTCHA", typeof(BlockSolveCaptcha) },
        { "REPORTCAPTCHA", typeof(BlockReportCaptcha) },
        { "FUNCTION", typeof(BlockFunction) },
        { "KEYCHECK", typeof(BlockKeycheck) },
        { "PARSE", typeof(BlockParse) },
        { "REQUEST", typeof(BlockRequest) },
        { "TCP", typeof(BlockTCP) },
        { "UTILITY", typeof(BlockUtility) },
        { "BROWSERACTION", typeof(SBlockBrowserAction) },
        { "ELEMENTACTION", typeof(SBlockElementAction) },
        { "EXECUTEJS", typeof(SBlockExecuteJS) },
        { "NAVIGATE", typeof(SBlockNavigate) }
    };

    /// <summary>
    /// Tests if a line is parsable as a block.
    /// </summary>
    /// <param name="line">The line to inspect.</param>
    /// <returns><see langword="true"/> if the line starts with a known block identifier; otherwise <see langword="false"/>.</returns>
    public static bool IsBlock(string line)
        => BlockMappings.Keys.Select(n => n.ToUpper()).Contains(GetBlockType(line).ToUpper());

    /// <summary>
    /// Gets the block type from a block line.
    /// </summary>
    /// <param name="line">The line to inspect.</param>
    /// <returns>The parsed block identifier.</returns>
    public static string GetBlockType(string line) => Regex.Match(line, @"^!?(#[^ ]* )?([^ ]*)").Groups[2].Value;

    /// <summary>
    /// Parses a block line as a block object.
    /// </summary>
    /// <param name="line">The LoliScript line to parse.</param>
    /// <returns>The parsed block instance.</returns>
    public static BlockBase Parse(string line)
    {
        // Trim the line
        var input = line.Trim();

        // Return an exception if the line is empty
        if (input == string.Empty)
        {
            throw new ArgumentNullException(nameof(line));
        }

        // Parse if disabled or not
        var disabled = input.StartsWith("!");

        if (disabled)
        {
            input = input[1..].Trim();
        }

        var label = LineParser.ParseToken(ref input, TokenType.Label, false);

        // Parse the identifier
        string identifier;

        try
        {
            identifier = LineParser.ParseToken(ref input, TokenType.Parameter, true);
        }
        catch
        {
            throw new ArgumentException("Missing identifier");
        }

        // Create the actual block from the identifier
        if (!BlockMappings.TryGetValue(identifier, out var blockType))
        {
            throw new ArgumentException($"Unknown identifier {identifier}");
        }

        var block = Activator.CreateInstance(blockType) as BlockBase;
        if (block is null)
        {
            throw new InvalidOperationException($"Could not create a block instance for identifier {identifier}");
        }

        block = block.FromLS(input)
            ?? throw new InvalidOperationException($"Could not parse the block instance for identifier {identifier}");

        // Set disabled
        block.Disabled = disabled;

        // Set the label
        if (label != string.Empty)
        {
            block.Label = label.Replace("#", "");
        }

        return block;
    }
}
