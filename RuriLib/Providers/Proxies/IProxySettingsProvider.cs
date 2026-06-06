using System;

namespace RuriLib.Providers.Proxies;

/// <summary>
/// Provides proxy-related settings and ban/retry key detection.
/// </summary>
public interface IProxySettingsProvider
{
    /// <summary>
    /// Gets the proxy connection timeout.
    /// </summary>
    TimeSpan ConnectTimeout { get; }

    /// <summary>
    /// Gets the proxy read/write timeout.
    /// </summary>
    TimeSpan ReadWriteTimeout { get; }

    /// <summary>
    /// Checks whether the given text contains a global ban key.
    /// </summary>
    bool ContainsBanKey(string text, out string matchedKey, bool caseSensitive = false);

    /// <summary>
    /// Checks whether the given text contains a global retry key.
    /// </summary>
    bool ContainsRetryKey(string text, out string matchedKey, bool caseSensitive = false);
}
