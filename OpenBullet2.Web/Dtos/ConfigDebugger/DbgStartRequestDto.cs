using RuriLib.Models.Proxies;

namespace OpenBullet2.Web.Dtos.ConfigDebugger;

/// <summary>
/// Request to start a new debugger for a given config.
/// </summary>
public class DbgStartRequestDto
{
    /// <summary>
    /// The data under test.
    /// </summary>
    public string TestData { get; set; } = "";

    /// <summary>
    /// The Wordlist Type to use when slicing the <see cref="TestData" />.
    /// </summary>
    public string WordlistType { get; set; } = "Default";

    /// <summary>
    /// Whether the provided <see cref="TestProxy" /> should be used.
    /// </summary>
    public bool UseProxy { get; set; } = false;

    /// <summary>
    /// The proxy to use for remote connections.
    /// </summary>
    public string TestProxy { get; set; } = "";

    /// <summary>
    /// The type of <see cref="TestProxy" />.
    /// </summary>
    public ProxyType ProxyType { get; set; } = ProxyType.Http;

    /// <summary>
    /// Whether to persist the logs from the previous debug.
    /// </summary>
    public bool PersistLog { get; set; } = false;

    /// <summary>
    /// Whether to debug the config in step by step mode, waiting for
    /// user input before proceeding with the next block.
    /// </summary>
    public bool StepByStep { get; set; } = false;
}
