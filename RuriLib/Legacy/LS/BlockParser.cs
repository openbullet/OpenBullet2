using RuriLib.Legacy.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Legacy.LS
{
    /// <summary>
    /// Parses a block from LoliScript code.
    /// </summary>
    public static class BlockParser
    {
        /// <summary>
        /// The allowed block identifiers.
        /// </summary>
        public static Dictionary<string, Type> BlockMappings { get; set; } = new Dictionary<string, Type>()
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
        public static bool IsBlock(string line)
            => BlockMappings.Keys.Select(n => n.ToUpper()).Contains(GetBlockType(line).ToUpper());

        /// <summary>
        /// Gets the block type from a block line.
        /// </summary>
        public static string GetBlockType(string line) => Regex.Match(line, @"^!?(#[^ ]* )?([^ ]*)").Groups[2].Value;

        /// <summary>
        /// Parses a block line as a block object.
        /// </summary>
        public static BlockBase Parse(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Return an exception if the line is empty
            if (input == string.Empty)
            {
                throw new ArgumentNullException();
            }

            // Parse if disabled or not
            var disabled = input.StartsWith("!");
            
            if (disabled)
            {
                input = input[1..].Trim();
            }

            var label = LineParser.ParseToken(ref input, TokenType.Label, false);

            // Parse the identifier
            var identifier = "";

            try
            {
                identifier = LineParser.ParseToken(ref input, TokenType.Parameter, true);
            }
            catch
            {
                throw new ArgumentException("Missing identifier");
            }

            // Create the actual block from the identifier
            var block = (Activator.CreateInstance(BlockMappings[identifier]) as BlockBase).FromLS(input);
            
            // Set disabled
            if (block != null)
            {
                block.Disabled = disabled;
            }

            // Set the label
            if (block != null && label != string.Empty)
            {
                block.Label = label.Replace("#", "");
            }

            return block;
        }
    }
}
