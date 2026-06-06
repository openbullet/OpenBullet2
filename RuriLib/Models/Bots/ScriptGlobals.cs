using System;
using System.Collections.Generic;
using System.Dynamic;

namespace RuriLib.Models.Bots;

/// <summary>
/// Global variables accessible by the Roslyn script.
/// </summary>
public class ScriptGlobals
{
    /// <summary>
    /// The data of the bot, such as the current DataLine or Proxy being used.
    /// </summary>
    public BotData data;

    /// <summary>
    /// The expando object where each field is a slice of the original data line.
    /// </summary>
    public dynamic input;

    /// <summary>
    /// The expando object where global variables are stored.
    /// </summary>
    public dynamic globals;

    /// <summary>
    /// Initializes the globals exposed to a Roslyn script.
    /// </summary>
    /// <param name="data">The current bot data.</param>
    /// <param name="globals">The shared global variables object.</param>
    public ScriptGlobals(BotData data, dynamic globals)
    {
        this.data = data;
        this.globals = globals;

        input = new ExpandoObject();

        foreach (var variable in data.Line.GetVariables())
        {
            ((IDictionary<string, object?>)input).Add(
                variable.Name,
                data.ConfigSettings.DataSettings.UrlEncodeDataAfterSlicing
                    ? Uri.EscapeDataString(variable.AsString())
                    : variable.AsString());
        }
    }
}
