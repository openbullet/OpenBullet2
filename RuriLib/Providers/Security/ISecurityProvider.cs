using System.Security.Cryptography.X509Certificates;

namespace RuriLib.Providers.Security;

/// <summary>
/// Provides security-related settings for block execution.
/// </summary>
public interface ISecurityProvider
{
    /// <summary>
    /// Gets a value indicating whether blocks are restricted to the current working directory.
    /// </summary>
    bool RestrictBlocksToCWD { get; }

    /// <summary>
    /// Gets or sets the certificate revocation mode used for TLS validation.
    /// </summary>
    X509RevocationMode X509RevocationMode { get; set; }
}
