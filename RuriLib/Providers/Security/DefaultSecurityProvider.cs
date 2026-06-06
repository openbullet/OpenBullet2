using RuriLib.Services;
using System.Security.Cryptography.X509Certificates;

namespace RuriLib.Providers.Security;

/// <summary>
/// Default implementation of <see cref="ISecurityProvider"/>.
/// </summary>
public class DefaultSecurityProvider : ISecurityProvider
{
    /// <summary>
    /// Gets a value indicating whether blocks are restricted to the current working directory.
    /// </summary>
    public bool RestrictBlocksToCWD { get; }

    /// <summary>
    /// Gets or sets the certificate revocation mode used for TLS validation.
    /// </summary>
    public X509RevocationMode X509RevocationMode { get; set; } = X509RevocationMode.NoCheck;

    /// <summary>
    /// Creates a provider from the persisted RuriLib settings.
    /// </summary>
    /// <param name="settings">The settings service to read from.</param>
    public DefaultSecurityProvider(RuriLibSettingsService settings)
    {
        RestrictBlocksToCWD = settings.RuriLibSettings.GeneralSettings.RestrictBlocksToCWD;
    }
}
