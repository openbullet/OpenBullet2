using RuriLib.Legacy.LS;
using RuriLib.Models.Bots;
using System.Collections.Generic;

namespace RuriLib.Legacy.Models;

/// <summary>
/// Provides the legacy global state shared while executing a LoliScript.
/// </summary>
public class LSGlobals
{
    /// <summary>
    /// Gets or sets the current bot runtime state.
    /// </summary>
    public BotData BotData { get; set; }

    /// <summary>
    /// Gets or sets the legacy global variables.
    /// </summary>
    public VariablesList Globals { get; set; } = new();

    /// <summary>
    /// Gets or sets the global cookie store used by legacy blocks.
    /// </summary>
    public Dictionary<string, string> GlobalCookies { get; set; } = new();

    /// <summary>
    /// Initializes the legacy global state.
    /// </summary>
    /// <param name="data">The current bot data.</param>
    public LSGlobals(BotData data)
    {
        BotData = data;
    }
}
