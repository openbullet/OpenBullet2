using RuriLib.Legacy.Blocks;
using RuriLib.Legacy.Models;
using RuriLib.Logging;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;
using System;

namespace RuriLib.Legacy.LS
{
    /// <summary>
    /// Parses a SET command.
    /// </summary>
    internal class SetParser
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
                switch (field)
                {
                    case "SOURCE":
                        data.SOURCE = LineParser.ParseLiteral(ref input, "SOURCE", true, ls);
                        break;

                    case "STATUS":
                        data.STATUS = LineParser.ParseToken(ref input, TokenType.Parameter, true);

                        // E.g. user wrote SET STATUS CUSTOM "TEST"
                        if (data.STATUS == "CUSTOM" && !string.IsNullOrEmpty(input))
                        {
                            data.STATUS = LineParser.ParseLiteral(ref input, "CUSTOM STATUS");
                        }
                        break;

                    case "RESPONSECODE":
                        data.RESPONSECODE = LineParser.ParseInt(ref input, "RESPONSECODE");
                        break;

                    case "COOKIE":
                        var name = LineParser.ParseLiteral(ref input, "NAME", true, ls);
                        data.COOKIES.Add(name, LineParser.ParseLiteral(ref input, "VALUE", true, ls));
                        break;

                    case "ADDRESS":
                        data.ADDRESS = LineParser.ParseLiteral(ref input, "ADDRESS", true, ls);
                        break;

                    case "USEPROXY":
                        var use = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();
                        
                        if (use == "TRUE")
                        {
                            data.UseProxy = true;
                        }
                        else if (use == "FALSE")
                        {
                            data.UseProxy = false;
                        }
                        break;

                    case "PROXY":
                        var prox = LineParser.ParseLiteral(ref input, "PROXY", true, ls);
                        data.Proxy = Proxy.Parse(prox);
                        break;

                    case "PROXYTYPE":
                        data.Proxy.Type = (ProxyType)LineParser.ParseEnum(ref input, "PROXYTYPE", typeof(ProxyType));
                        break;

                    case "DATA":
                        data.Line.Data = LineParser.ParseLiteral(ref input, "DATA", true, ls);
                        break;

                    case "VAR":
                        var varName = LineParser.ParseLiteral(ref input, "NAME", true, ls);
                        var varValue = LineParser.ParseLiteral(ref input, "VALUE", true, ls);
                        BlockBase.GetVariables(data).Set(new StringVariable(varValue)
                        {
                            Name = varName
                        });
                        break;

                    case "CAP":
                        var capName = LineParser.ParseLiteral(ref input, "NAME", true, ls);
                        var capValue = LineParser.ParseLiteral(ref input, "VALUE", true, ls);
                        BlockBase.GetVariables(data).Set(new StringVariable(capValue)
                        {
                            Name = capName
                        });
                        data.MarkForCapture(capName);
                        break;

                    case "GVAR":
                        try
                        {
                            var globalVarName = LineParser.ParseLiteral(ref input, "NAME", true, ls);
                            var globalVarValue = LineParser.ParseLiteral(ref input, "VALUE", true, ls);
                            ls.Globals.Set(new StringVariable(globalVarValue)
                            {
                                Name = globalVarName
                            });
                        }
                        catch
                        {

                        }
                        break;

                    case "NEWGVAR":
                        try
                        {
                            var globalVarName = LineParser.ParseLiteral(ref input, "NAME", true, ls);
                            var globalVarValue = LineParser.ParseLiteral(ref input, "VALUE", true, ls);
                            ls.Globals.SetIfNew(new StringVariable(globalVarValue)
                            {
                                Name = globalVarName
                            });
                        }
                        catch
                        {

                        }
                        break;

                    case "GCOOKIES":
                        ls.GlobalCookies.Clear();
                        foreach (var cookie in data.COOKIES)
                        {
                            ls.GlobalCookies.Add(cookie.Key, cookie.Value);
                        }
                        break;

                    default:
                        throw new ArgumentException($"Invalid identifier {field}");
                }

                data.Logger.Log($"SET command executed on field {field}", LogColors.White);
            });
        }
    }
}
