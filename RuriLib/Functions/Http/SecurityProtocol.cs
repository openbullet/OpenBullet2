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
        TLS10,

        /// <summary>The TLS 1.1 protocol.</summary>
        TLS11,

        /// <summary>The TLS 1.2 protocol.</summary>
        TLS12,

        /// <summary>The TLS 1.3 protocol.</summary>
        TLS13
    }
}
