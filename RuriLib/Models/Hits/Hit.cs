using RuriLib.Logging;
using RuriLib.Models.Configs;
using RuriLib.Models.Data;
using RuriLib.Models.Environment;
using RuriLib.Models.Proxies;
using RuriLib.Models.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RuriLib.Models.Hits;

/// <summary>
/// Represents a stored hit or check result.
/// </summary>
public class Hit
{
    /// <summary>Gets the unique identifier.</summary>
    public string Id { get; } = Guid.NewGuid().ToString();
    /// <summary>Gets or sets the source data line.</summary>
    public DataLine Data { get; set; } = new(string.Empty, new WordlistType());
    /// <summary>Gets the serialized source data.</summary>
    public string DataString => Data.Data;
    /// <summary>Gets or sets the captured output variables.</summary>
    public Dictionary<string, object> CapturedData { get; set; } = [];
    /// <summary>Gets the formatted captured data string.</summary>
    public string CapturedDataString => ConvertCapturedData();
    /// <summary>Gets or sets the proxy used for the hit.</summary>
    public Proxy? Proxy { get; set; }
    /// <summary>Gets the proxy string representation.</summary>
    public string ProxyString => Proxy?.ToString() ?? string.Empty;
    /// <summary>Gets or sets the hit timestamp.</summary>
    public DateTime Date { get; set; }
    /// <summary>Gets or sets the hit type or status.</summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>Gets or sets the originating config.</summary>
    public Config Config { get; set; } = new() { Id = string.Empty };
    /// <summary>Gets or sets the originating data pool.</summary>
    public DataPool? DataPool { get; set; }
    /// <summary>Gets or sets the optional bot logger.</summary>
    public IBotLogger? BotLogger { get; set; }
    /// <summary>Gets or sets the owner identifier.</summary>
    public int OwnerId { get; set; } = -1;

    /// <summary>
    /// Returns a compact string representation of the hit.
    /// </summary>
    /// <returns>The formatted hit string.</returns>
    public override string ToString()
        => string.Join(" | ", new[] { DataString, CapturedDataString }.Where(s => !string.IsNullOrEmpty(s)));

    private string ConvertCapturedData()
    {
        var variables = new List<Variable>();

        foreach (var data in CapturedData)
        {
            try
            {
                var variable = VariableFactory.FromObject(data.Value);
                variable.Name = data.Key;
                variables.Add(variable);
            }
            catch
            {
                // If the variable is null, the snippet above will throw an exception, so just
                // add a dummy string variable with the literal value "null".
                variables.Add(new StringVariable("null") { Name = data.Key });
            }
        }

        return string.Join(" | ", variables.Select(v => $"{v.Name} = {v.AsString()}"));
    }
}
