using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace RuriLib.LS
{
    /// <summary>
    /// Parse a command from LoliScript code.
    /// </summary>
    public class CommandParser
    {
        /// <summary>
        /// The allowed command identifiers.
        /// </summary>
        public enum CommandName
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
        public static bool IsCommand(string line)
        {
            var groups = Regex.Match(line, @"^([^ ]*)").Groups;
            return Enum.GetNames(typeof(CommandName)).Select(n => n.ToUpper()).Contains(groups[1].Value.ToUpper());
        }

        /// <summary>
        /// Gets a command Action from a command line.
        /// </summary>
        /// <param name="line">The command line</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        /// <returns>The Action that needs to be executed</returns>
        public static Action Parse(string line, BotData data)
        {
            // Trim the line
            var input = line.Trim();

            // Return an exception if the line is empty
            if (input == string.Empty) throw new ArgumentNullException();

            var label = LineParser.ParseToken(ref input, TokenType.Label, false);

            // Parse the identifier
            var identifier = "";
            try { identifier = LineParser.ParseToken(ref input, TokenType.Parameter, true); }
            catch { throw new ArgumentException("Missing identifier"); }

            switch ((CommandName)Enum.Parse(typeof(CommandName), identifier, true))
            {
                case CommandName.PRINT:
                    return new Action(() => data.Log(new LogEntry(BlockBase.ReplaceValues(input, data), Colors.White)));

                case CommandName.SET:
                    return SetParser.Parse(input, data);

                case CommandName.DELETE:
                    return DeleteParser.Parse(input, data);

                case CommandName.MOUSEACTION:
                    return MouseActionParser.Parse(input, data);

                default:
                    throw new ArgumentException($"Invalid identifier '{identifier}'");
            }
        }
    }
}
