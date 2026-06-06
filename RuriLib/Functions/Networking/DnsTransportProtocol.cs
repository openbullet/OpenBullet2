using System.ComponentModel;

namespace RuriLib.Functions.Networking;

/// <summary>
/// Supported DNS transports.
/// </summary>
public enum DnsTransportProtocol
{
    /// <summary>DNS over HTTPS.</summary>
    [Description("DNS over HTTPS")]
    DnsOverHttps,

    /// <summary>Plain DNS over UDP.</summary>
    [Description("UDP")]
    Udp,

    /// <summary>Plain DNS over TCP.</summary>
    [Description("TCP")]
    Tcp
}
