namespace RuriLib.Functions.Networking;

/// <summary>
/// Represents a host and port pair.
/// </summary>
public struct HostEntry
{
    /// <summary>
    /// Gets or sets the host name.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Creates a host and port pair.
    /// </summary>
    public HostEntry(string host, int port)
    {
        Host = host;
        Port = port;
    }
}
