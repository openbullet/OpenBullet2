using RuriLib.Exceptions;
using RuriLib.Helpers.Blocks;
using RuriLib.Models.Blocks;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Helpers.Transpilers
{
    /// <summary>
    /// Takes care of transpiling LoliCode to a list of blocks.
    /// </summary>
    public static class Loli2StackTranspiler
    {
        private static readonly string validTokenRegex = "[A-Za-z][A-Za-z0-9_]*";

        /// <summary>
        /// Creates a list of <see cref="BlockInstance"/> objects from a LoliCode <paramref name="script"/>.
        /// </summary>
        public static List<BlockInstance> Transpile(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                return new List<BlockInstance>();

            var lines = script.Split(new string[] { "\n", "\r\n" }, System.StringSplitOptions.None);
            var stack = new List<BlockInstance>();

            var localLineNumber = 0;
            var lineNumber = 0;

            while (localLineNumber < lines.Length)
            {
                var line = lines[localLineNumber];
                var trimmedLine = line.Trim();
                localLineNumber++;
                lineNumber++;

                // If it's a block directive
                if (trimmedLine.StartsWith("BLOCK:"))
                {
                    /* 
                        BLOCK:Id
                        ...
                        ENDBLOCK
                    */

                    var match = Regex.Match(trimmedLine, $"^BLOCK:({validTokenRegex})$");

                    if (!match.Success)
                        throw new LoliCodeParsingException(lineNumber, "Could not parse the block id");

                    var blockId = match.Groups[1].Value;

                    // Create the block
                    var block = BlockFactory.GetBlock<BlockInstance>(blockId);

                    var sb = new StringBuilder();

                    // As long as we don't find the ENDBLOCK token, add lines to the StringBuilder
                    while (localLineNumber < lines.Length)
                    {
                        line = lines[localLineNumber];
                        trimmedLine = line.Trim();
                        localLineNumber++;

                        if (trimmedLine.StartsWith("ENDBLOCK"))
                            break;

                        sb.AppendLine(line);
                    }

                    var blockOptions = sb.ToString();
                    block.FromLC(ref blockOptions, ref lineNumber); // This can throw a LoliCodeParsingException
                    lineNumber++; // Add one line for the ENDBLOCK statement
                    
                    stack.Add(block);
                }

                // If it's not a block directive, build a LoliCode block
                else
                {
                    var descriptor = new LoliCodeBlockDescriptor();
                    var block = new LoliCodeBlockInstance(descriptor);

                    var sb = new StringBuilder();

                    sb.Append(line);

                    // As long as we don't find a BLOCK directive, add lines to the StringBuilder
                    while (localLineNumber < lines.Length)
                    {
                        sb.AppendLine(); // Print a newline character
                        line = lines[localLineNumber];
                        trimmedLine = line.Trim();
                        lineNumber++;
                        localLineNumber++;

                        // If we find a block directive, stop reading lines without consuming it
                        if (trimmedLine.StartsWith("BLOCK:"))
                        {
                            lineNumber--;
                            localLineNumber--;
                            break;
                        }

                        sb.Append(line);
                    }

                    block.Script = sb.ToString();

                    // Make sure the script is not empty
                    if (!string.IsNullOrWhiteSpace(block.Script.Replace("\n", "").Replace("\r\n", "")))
                        stack.Add(block);
                }
            }

            return stack;
        }
    }
}
