using System;
using System.Collections.Generic;

namespace RuriLib.Legacy.Configs;

/// <summary>
/// Stores the settings block of a legacy OpenBullet config.
/// </summary>
public class LegacyConfigSettings
{
    // General
    /// <summary>
    /// The config name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The suggested bot count for the config.
    /// </summary>
    public int SuggestedBots { get; set; }

    /// <summary>
    /// The maximum suggested CPM.
    /// </summary>
    public int MaxCPM { get; set; }

    /// <summary>
    /// The last modification timestamp.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Additional readme-like information for the config.
    /// </summary>
    public string AdditionalInfo { get; set; } = string.Empty;

    /// <summary>
    /// The plugin identifiers required by the config.
    /// </summary>
    public string[] RequiredPlugins { get; set; } = [];

    /// <summary>
    /// The author name.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// The legacy version string.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Whether empty captures should be stored.
    /// </summary>
    public bool SaveEmptyCaptures { get; set; }

    /// <summary>
    /// Whether execution should continue on custom status.
    /// </summary>
    public bool ContinueOnCustom { get; set; }

    /// <summary>
    /// Whether hits should be saved to a text file.
    /// </summary>
    public bool SaveHitsToTextFile { get; set; }

    // Requests
    /// <summary>
    /// Whether request errors should be ignored.
    /// </summary>
    public bool IgnoreResponseErrors { get; set; }

    /// <summary>
    /// The maximum redirect count.
    /// </summary>
    public int MaxRedirects { get; set; }

    // Proxy
    /// <summary>
    /// Whether the config requires proxies.
    /// </summary>
    public bool NeedsProxies { get; set; }

    /// <summary>
    /// Whether only SOCKS proxies are allowed.
    /// </summary>
    public bool OnlySocks { get; set; }

    /// <summary>
    /// Whether only SSL-compatible proxies are allowed.
    /// </summary>
    public bool OnlySsl { get; set; }

    /// <summary>
    /// The maximum number of uses per proxy.
    /// </summary>
    public int MaxProxyUses { get; set; }

    /// <summary>
    /// Whether to ban a proxy after a good status.
    /// </summary>
    public bool BanProxyAfterGoodStatus { get; set; }

    /// <summary>
    /// The legacy ban-loop evasion override. A value of <c>-1</c> means disabled.
    /// </summary>
    public int BanLoopEvasionOverride { get; set; } = -1;

    // Data
    /// <summary>
    /// Whether sliced data should be URL-encoded.
    /// </summary>
    public bool EncodeData { get; set; }

    /// <summary>
    /// The primary allowed wordlist type.
    /// </summary>
    public string AllowedWordlist1 { get; set; } = string.Empty;

    /// <summary>
    /// The secondary allowed wordlist type.
    /// </summary>
    public string AllowedWordlist2 { get; set; } = string.Empty;

    /// <summary>
    /// The legacy data rules.
    /// </summary>
    public List<LegacyDataRule> DataRules { get; set; } = [];

    // Inputs
    /// <summary>
    /// The legacy custom inputs.
    /// </summary>
    public List<LegacyCustomInput> CustomInputs { get; set; } = [];

    // Selenium
    /// <summary>
    /// Whether browsers should be forced to run headless.
    /// </summary>
    public bool ForceHeadless { get; set; }

    /// <summary>
    /// Whether the browser should always be opened.
    /// </summary>
    public bool AlwaysOpen { get; set; }

    /// <summary>
    /// Whether the browser should always be closed after use.
    /// </summary>
    public bool AlwaysQuit { get; set; }

    /// <summary>
    /// Whether the browser should quit on ban or retry.
    /// </summary>
    public bool QuitOnBanRetry { get; set; }

    /// <summary>
    /// Whether browser notifications should be disabled.
    /// </summary>
    public bool DisableNotifications { get; set; }

    /// <summary>
    /// The custom user agent string.
    /// </summary>
    public string CustomUserAgent { get; set; } = string.Empty;

    /// <summary>
    /// Whether a random user agent should be used.
    /// </summary>
    public bool RandomUA { get; set; }

    /// <summary>
    /// Additional browser command-line arguments.
    /// </summary>
    public string CustomCMDArgs { get; set; } = string.Empty;
}
