using RuriLib.Legacy.Blocks;
using RuriLib.Legacy.Functions.Conditions;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using System;
using System.Linq;

namespace RuriLib.Legacy.LS
{
    /// <summary>
    /// Parses a DELETE command.
    /// </summary>
    internal class DeleteParser
    {
        /// <summary>
        /// Gets the Action that needs to be executed.
        /// </summary>
        static internal Action Parse(string line, LSGlobals ls)
        {
            var data = ls.BotData;
            var input = line.Trim();
            var field = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();

            return new Action(() =>
            {
                var name = "";
                var comparer = Comparer.EqualTo;

                switch (field)
                {
                    case "COOKIE":
                        if(LineParser.Lookahead(ref input) == TokenType.Parameter)
                        {
                            comparer = (Comparer)LineParser.ParseEnum(ref input, "TYPE", typeof(Comparer));
                        }
                        
                        name = LineParser.ParseLiteral(ref input, "NAME");

                        for (var i = 0; i < data.COOKIES.Count; i++)
                        {
                            var curr = data.COOKIES.ToList()[i].Key;

                            if (Condition.ReplaceAndVerify(curr, comparer, name, ls))
                            {
                                data.COOKIES.Remove(curr);
                            }
                        }
                        break;

                    case "VAR":
                        if (LineParser.Lookahead(ref input) == TokenType.Parameter)
                        {
                            comparer = (Comparer)LineParser.ParseEnum(ref input, "TYPE", typeof(Comparer));
                        }

                        name = LineParser.ParseLiteral(ref input, "NAME");
                        BlockBase.GetVariables(data).RemoveAll(comparer, name, ls);
                        break;

                    case "GVAR":
                        if (LineParser.Lookahead(ref input) == TokenType.Parameter)
                        {
                            comparer = (Comparer)LineParser.ParseEnum(ref input, "TYPE", typeof(Comparer));
                        }

                        name = LineParser.ParseLiteral(ref input, "NAME");
                        
                        try
                        {
                            ls.Globals.RemoveAll(comparer, name, ls);
                        }
                        catch
                        {

                        }
                        break;

                    default:
                        throw new ArgumentException($"Invalid identifier {field}");
                }

                data.Logger.Log($"DELETE command executed on field {field}", LogColors.White);
            });
        }
    }
}
