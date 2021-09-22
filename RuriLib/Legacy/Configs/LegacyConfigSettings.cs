using System;
using System.Collections.Generic;

namespace RuriLib.Legacy.Configs
{
    internal class LegacyConfigSettings
    {
        // General
        internal string Name { get; set; }
        internal int SuggestedBots { get; set; }
        internal int MaxCPM { get; set; }
        internal DateTime LastModified { get; set; }
        internal string AdditionalInfo { get; set; }
        internal string[] RequiredPlugins { get; set; }
        internal string Author { get; set; }
        internal string Version { get; set; }
        internal bool SaveEmptyCaptures { get; set; }
        internal bool ContinueOnCustom { get; set; }
        internal bool SaveHitsToTextFile { get; set; }

        // Requests
        internal bool IgnoreResponseErrors { get; set; }
        internal int MaxRedirects { get; set; }

        // Proxy
        internal bool NeedsProxies { get; set; }
        internal bool OnlySocks { get; set; }
        internal bool OnlySsl { get; set; }
        internal int MaxProxyUses { get; set; }
        internal bool BanProxyAfterGoodStatus { get; set; }
        internal int BanLoopEvasionOverride { get; set; } = -1;

        // Data
        internal bool EncodeData { get; set; }
        internal string AllowedWordlist1 { get; set; }
        internal string AllowedWordlist2 { get; set; }
        internal List<LegacyDataRule> DataRules { get; set; } = new();

        // Inputs
        internal List<LegacyCustomInput> CustomInputs { get; set; } = new();

        // Selenium
        internal bool ForceHeadless { get; set; }
        internal bool AlwaysOpen { get; set; }
        internal bool AlwaysQuit { get; set; }
        internal bool QuitOnBanRetry { get; set; }
        internal bool DisableNotifications { get; set; }
        internal string CustomUserAgent { get; set; }
        internal bool RandomUA { get; set; }
        internal string CustomCMDArgs { get; set; }
    }
}
