using Extreme.Net;
using RuriLib.Models;
using System;
using System.Windows.Media;

namespace RuriLib.LS
{
    /// <summary>
    /// Parses a SET command.
    /// </summary>
    class SetParser
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
                switch (field)
                {
                    case "SOURCE":
                        data.ResponseSource = LineParser.ParseLiteral(ref input, "SOURCE", true, data);
                        break;

                    case "STATUS":
                        data.Status = (BotStatus)LineParser.ParseEnum(ref input, "STATUS", typeof(BotStatus));
                        if (data.Status == BotStatus.CUSTOM) data.CustomStatus = LineParser.ParseLiteral(ref input, "CUSTOM STATUS");
                        break;

                    case "RESPONSECODE":
                        data.ResponseCode = LineParser.ParseInt(ref input, "RESPONSECODE").ToString();
                        break;

                    case "COOKIE":
                        var name = LineParser.ParseLiteral(ref input, "NAME", true, data);
                        data.Cookies.Add(name, LineParser.ParseLiteral(ref input, "VALUE", true, data));
                        break;

                    case "ADDRESS":
                        data.Address = LineParser.ParseLiteral(ref input, "ADDRESS", true, data);
                        break;

                    case "USEPROXY":
                        var use = LineParser.ParseToken(ref input, TokenType.Parameter, true).ToUpper();
                        if (use == "TRUE") data.UseProxies = true;
                        else if (use == "FALSE") data.UseProxies = false;
                        break;

                    case "PROXY":
                        var prox = LineParser.ParseLiteral(ref input, "PROXY", true, data);
                        data.Proxy = new CProxy(BlockBase.ReplaceValues(prox, data), data.Proxy == null ? ProxyType.Http : data.Proxy.Type);
                        break;

                    case "PROXYTYPE":
                        data.Proxy.Type = (ProxyType)LineParser.ParseEnum(ref input, "PROXYTYPE", typeof(ProxyType));
                        break;

                    case "DATA":
                        data.Data = new CData(LineParser.ParseLiteral(ref input, "DATA", true, data), new WordlistType());
                        break;

                    case "VAR":
                        data.Variables.Set(new CVar(
                            LineParser.ParseLiteral(ref input, "NAME", true, data),
                            LineParser.ParseLiteral(ref input, "VALUE", true, data),
                            false
                            ));
                        break;

                    case "CAP":
                        data.Variables.Set(new CVar(
                            LineParser.ParseLiteral(ref input, "NAME", true, data),
                            LineParser.ParseLiteral(ref input, "VALUE", true, data),
                            true
                            ));
                        break;

                    case "GVAR":
                        try
                        {
                            data.GlobalVariables.Set(new CVar(
                                LineParser.ParseLiteral(ref input, "NAME", true, data),
                                LineParser.ParseLiteral(ref input, "VALUE", true, data),
                                false
                                ));
                        }
                        catch { }
                        break;

                    case "NEWGVAR":
                        try
                        {
                            data.GlobalVariables.SetNew(new CVar(
                                LineParser.ParseLiteral(ref input, "NAME", true, data),
                                LineParser.ParseLiteral(ref input, "VALUE", true, data),
                                false
                                ));
                        }
                        catch { }
                        break;

                    case "GCOOKIES":
                        data.GlobalCookies.Clear();
                        foreach (var cookie in data.Cookies)
                            data.GlobalCookies.Add(cookie.Key, cookie.Value);
                        break;

                    default:
                        throw new ArgumentException($"Invalid identifier {field}");
                }

                data.Log(new LogEntry($"SET command executed on field {field}", Colors.White));
            });
        }
    }
}
