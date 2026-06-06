using System.ComponentModel;

namespace RuriLib.Functions.Networking;

/// <summary>
/// Supported DNS record types for queries.
/// </summary>
public enum DnsRecordType
{
    /// <summary>An IPv4 address record.</summary>
    [Description("A")]
    A,

    /// <summary>An IPv6 address record.</summary>
    [Description("AAAA")]
    AAAA,

    /// <summary>A certification authority authorization record.</summary>
    [Description("CAA")]
    CAA,

    /// <summary>A canonical name record.</summary>
    [Description("CNAME")]
    CNAME,

    /// <summary>A mail exchange record.</summary>
    [Description("MX")]
    MX,

    /// <summary>A naming authority pointer record.</summary>
    [Description("NAPTR")]
    NAPTR,

    /// <summary>An authoritative name server record.</summary>
    [Description("NS")]
    NS,

    /// <summary>A pointer record.</summary>
    [Description("PTR")]
    PTR,

    /// <summary>A start of authority record.</summary>
    [Description("SOA")]
    SOA,

    /// <summary>A service locator record.</summary>
    [Description("SRV")]
    SRV,

    /// <summary>A TLSA certificate association record.</summary>
    [Description("TLSA")]
    TLSA,

    /// <summary>A text record.</summary>
    [Description("TXT")]
    TXT,

    /// <summary>A URI record.</summary>
    [Description("URI")]
    URI
}
