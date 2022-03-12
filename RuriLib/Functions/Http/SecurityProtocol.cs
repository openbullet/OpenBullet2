using System.ComponentModel;

namespace RuriLib.Functions.Http
{
    /// <summary>
    /// Enumerates the supported security protocols.
    /// </summary>
    public enum SecurityProtocol
    {
        /// <summary>Let the operative system decide and block the unsecure protocols.</summary>
        SystemDefault,

        /// <summary>The TLS 1.0 protocol (obsolete).</summary>
        [Description("TLS 1.0")]
        TLS10,

        /// <summary>The TLS 1.1 protocol.</summary>
        [Description("TLS 1.1")]
        TLS11,

        /// <summary>The TLS 1.2 protocol.</summary>
        [Description("TLS 1.2")]
        TLS12,

        /// <summary>The TLS 1.3 protocol.</summary>
        [Description("TLS 1.3")]
        TLS13
    }
}
