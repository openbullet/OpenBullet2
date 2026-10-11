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

        var inputFields = (IDictionary<string, object?>)input;

        // Expose the proxy assigned to this bot as input.proxy.* so that configs can
        // read its fields without having to fall back to data.Proxy. A wordlist slice
        // named "proxy" takes precedence so existing configs keep working.
        if (!inputFields.ContainsKey("proxy"))
        {
            inputFields.Add("proxy", CreateProxyVariables(data));
        }
    }

    /// <summary>
    /// Builds the <c>input.proxy</c> object. Every field is always present, defaulting
    /// to an empty value when the bot has no proxy assigned, so that configs never have
    /// to null-check before interpolating them.
    /// </summary>
    /// <param name="data">The bot data holding the proxy assigned to this bot.</param>
    /// <returns>An expando object with the fields of the proxy.</returns>
    private static ExpandoObject CreateProxyVariables(BotData data)
    {
        var proxy = data.Proxy;

        var variables = new ExpandoObject();
        var fields = (IDictionary<string, object?>)variables;

        fields["ip"] = proxy?.Host ?? string.Empty;
        fields["host"] = proxy?.Host ?? string.Empty;
        fields["port"] = proxy?.Port ?? 0;
        fields["username"] = proxy?.Username ?? string.Empty;
        fields["password"] = proxy?.Password ?? string.Empty;
        fields["type"] = proxy?.Type.ToString().ToLowerInvariant() ?? string.Empty;
        fields["country"] = proxy?.Country ?? string.Empty;
        fields["ping"] = proxy?.Ping ?? 0;
        fields["inUse"] = data.UseProxy && proxy is not null;

        return variables;
    }
}
