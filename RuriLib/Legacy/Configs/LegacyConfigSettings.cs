using System;
using System.Collections.Generic;

namespace RuriLib.Legacy.Configs
{
    public class LegacyConfigSettings
    {
        // General
        public string Name { get; set; }
        public int SuggestedBots { get; set; }
        public int MaxCPM { get; set; }
        public DateTime LastModified { get; set; }
        public string AdditionalInfo { get; set; }
        public string[] RequiredPlugins { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public bool SaveEmptyCaptures { get; set; }
        public bool ContinueOnCustom { get; set; }
        public bool SaveHitsToTextFile { get; set; }

        // Requests
        public bool IgnoreResponseErrors { get; set; }
        public int MaxRedirects { get; set; }

        // Proxy
        public bool NeedsProxies { get; set; }
        public bool OnlySocks { get; set; }
        public bool OnlySsl { get; set; }
        public int MaxProxyUses { get; set; }
        public bool BanProxyAfterGoodStatus { get; set; }
        public int BanLoopEvasionOverride { get; set; } = -1;

        // Data
        public bool EncodeData { get; set; }
        public string AllowedWordlist1 { get; set; }
        public string AllowedWordlist2 { get; set; }
        public List<LegacyDataRule> DataRules { get; set; } = new();

        // Inputs
        public List<LegacyCustomInput> CustomInputs { get; set; } = new();

        // Selenium
        public bool ForceHeadless { get; set; }
        public bool AlwaysOpen { get; set; }
        public bool AlwaysQuit { get; set; }
        public bool QuitOnBanRetry { get; set; }
        public bool DisableNotifications { get; set; }
        public string CustomUserAgent { get; set; }
        public bool RandomUA { get; set; }
        public string CustomCMDArgs { get; set; }
    }
}
