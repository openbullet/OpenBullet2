using RuriLib.Models.Settings;
using RuriLib.Services;
using System;
using System.Linq;

namespace RuriLib.Providers.Proxies;

/// <summary>
/// Default implementation of <see cref="IProxySettingsProvider"/>.
/// </summary>
public class DefaultProxySettingsProvider : IProxySettingsProvider
{
    private readonly ProxySettings settings;

    /// <summary>
    /// Creates a provider from the persisted RuriLib settings.
    /// </summary>
    public DefaultProxySettingsProvider(RuriLibSettingsService settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        this.settings = settings.RuriLibSettings.ProxySettings;
    }

    /// <summary>
    /// Gets the proxy connection timeout.
    /// </summary>
    public TimeSpan ConnectTimeout => TimeSpan.FromMilliseconds(settings.ProxyConnectTimeoutMilliseconds);

    /// <summary>
    /// Gets the proxy read/write timeout.
    /// </summary>
    public TimeSpan ReadWriteTimeout => TimeSpan.FromMilliseconds(settings.ProxyReadWriteTimeoutMilliseconds);

    /// <summary>
    /// Checks whether the given text contains a global ban key.
    /// </summary>
    public bool ContainsBanKey(string text, out string matchedKey, bool caseSensitive = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            matchedKey = string.Empty;
            return false;
        }

        matchedKey = settings.GlobalBanKeys
            .FirstOrDefault(k => !string.IsNullOrEmpty(k) && text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            ?? string.Empty;

        return matchedKey.Length != 0;
    }

    /// <summary>
    /// Checks whether the given text contains a global retry key.
    /// </summary>
    public bool ContainsRetryKey(string text, out string matchedKey, bool caseSensitive = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            matchedKey = string.Empty;
            return false;
        }

        matchedKey = settings.GlobalRetryKeys
            .FirstOrDefault(k => !string.IsNullOrEmpty(k) && text.Contains(k,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            ?? string.Empty;

        return matchedKey.Length != 0;
    }
}
