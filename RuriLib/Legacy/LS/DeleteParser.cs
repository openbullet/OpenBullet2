using RuriLib.Functions.Conditions;
using System;
using System.Linq;
using System.Windows.Media;

namespace RuriLib.LS
{
    /// <summary>
    /// Parses a DELETE command.
    /// </summary>
    class DeleteParser
    {
        /// <summary>
        /// Gets the Action that needs to be executed.
        /// </summary>
        /// <param name="line">The data line to parse</param>
        /// <param name="data">The BotData needed for variable replacement</param>
        /// <returns>The Action to execute</returns>
        public static Action Parse(string line, BotData data)
        {
            var input = line.Trim();
            var field = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();

            return new Action(() =>
            {
                var name = "";
                Comparer comparer = Comparer.EqualTo;

                switch (field)
                {
                    case "COOKIE":
                        if(LineParser.Lookahead(ref input) == TokenType.Parameter)
                            comparer = (Comparer)LineParser.ParseEnum(ref input, "TYPE", typeof(Comparer));
                        name = LineParser.ParseLiteral(ref input, "NAME");
                        for (int i=0; i<data.Cookies.Count; i++)
                        {
                            var curr = data.Cookies.ToList()[i].Key;
                            if (Condition.ReplaceAndVerify(curr, comparer, name, data)) data.Cookies.Remove(curr);
                        }
                        break;

                    case "VAR":
                        if (LineParser.Lookahead(ref input) == TokenType.Parameter)
                            comparer = (Comparer)LineParser.ParseEnum(ref input, "TYPE", typeof(Comparer));
                        name = LineParser.ParseLiteral(ref input, "NAME");
                        data.Variables.Remove(comparer, name, data);
                        break;

                    case "GVAR":
                        if (LineParser.Lookahead(ref input) == TokenType.Parameter)
                            comparer = (Comparer)LineParser.ParseEnum(ref input, "TYPE", typeof(Comparer));
                        name = LineParser.ParseLiteral(ref input, "NAME");
                        try
                        {
                            data.GlobalVariables.Remove(comparer, name, data);
                        }
                        catch { }
                        break;

                    default:
                        throw new ArgumentException($"Invalid identifier {field}");
                }

                data.Log(new LogEntry($"DELETE command executed on field {field}", Colors.White));
            });
        }
    }
}
