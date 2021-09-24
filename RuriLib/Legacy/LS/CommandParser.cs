using RuriLib.Legacy.Blocks;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace RuriLib.Legacy.LS
{
    /// <summary>
    /// Parse a command from LoliScript code.
    /// </summary>
    internal class CommandParser
    {
        /// <summary>
        /// The allowed command identifiers.
        /// </summary>
        enum CommandName
        {
            /// <summary>Prints some data to the log after replacing variables in it.</summary>
            PRINT,

            /// <summary>Sets the value of hidden variables.</summary>
            SET,

            /// <summary>Deletes variables.</summary>
            DELETE,

            /// <summary>Moves the mouse in a selenium-driven browser.</summary>
            MOUSEACTION
        }

        /// <summary>
        /// Tests if a line is parsable as a command.
        /// </summary>
        /// <param name="line">The data line to test</param>
        /// <returns>Whether the line contains a command or not</returns>
        internal static bool IsCommand(string line)
        {
            var groups = Regex.Match(line, @"^([^ ]*)").Groups;
            return Enum.GetNames(typeof(CommandName)).Select(n => n.ToUpper()).Contains(groups[1].Value.ToUpper());
        }

        /// <summary>
        /// Gets a command Action from a command line.
        /// </summary>
        internal static Action Parse(string line, LSGlobals ls)
        {
            // Trim the line
            var input = line.Trim();

            // Return an exception if the line is empty
            if (input == string.Empty)
            {
                throw new ArgumentNullException();
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

            return (CommandName)Enum.Parse(typeof(CommandName), identifier, true) switch
            {
                CommandName.PRINT => new Action(() => ls.BotData.Logger.Log(BlockBase.ReplaceValues(input, ls), LogColors.White)),
                CommandName.SET => SetParser.Parse(input, ls),
                CommandName.DELETE => DeleteParser.Parse(input, ls),
                // TODO: Readd this
                // CommandName.MOUSEACTION => MouseActionParser.Parse(input, ls),
                _ => throw new ArgumentException($"Invalid identifier '{identifier}'"),
            };
        }
    }
}
