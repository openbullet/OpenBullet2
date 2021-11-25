using Newtonsoft.Json;
using RuriLib.Extensions;
using RuriLib.Models.Configs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.Rules;
using RuriLib.Models.Proxies;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RuriLib.Legacy.Configs
{
    /// <summary>
    /// Maps settings of legacy configs to the new format.
    /// </summary>
    public static class ConfigConverter
    {
        public static Config Convert(string fileContent, string id)
        {
            // Deserialize the legacy config
            var split = fileContent.Split(new string[] { "[SETTINGS]", "[SCRIPT]" }, StringSplitOptions.RemoveEmptyEntries);
            var legacySettings = JsonConvert.DeserializeObject<LegacyConfigSettings>(split[0].TrimStart('\r', '\n'));
            var loliScript = split[1].TrimStart('\r', '\n');

            // Create a legacy config in the new format
            var newConfig = new Config
            {
                Id = id,
                Mode = ConfigMode.Legacy
            };

            // Convert the settings to new ones
            ApplyLegacySettings(newConfig, legacySettings);

            // Set the LoliScript
            newConfig.LoliScript = loliScript;

            return newConfig;
        }

        private static void ApplyLegacySettings(Config newConfig, LegacyConfigSettings legacySettings)
        {
            // Metadata
            newConfig.Metadata.Author = legacySettings.Author ?? "Unknown";
            newConfig.Metadata.Name = legacySettings.Name;
            
            // Readme
            newConfig.Readme = legacySettings.AdditionalInfo ?? "No information provided";

            // General
            newConfig.Settings.GeneralSettings.MaximumCPM = legacySettings.MaxCPM;
            newConfig.Settings.GeneralSettings.SaveEmptyCaptures = legacySettings.SaveEmptyCaptures;
            newConfig.Settings.GeneralSettings.SuggestedBots = legacySettings.SuggestedBots;

            if (legacySettings.ContinueOnCustom)
            {
                newConfig.Settings.GeneralSettings.ContinueStatuses = new string[] { "SUCCESS", "NONE", "CUSTOM" };
            }

            // Data
            newConfig.Settings.DataSettings.AllowedWordlistTypes = new string[] { legacySettings.AllowedWordlist1, legacySettings.AllowedWordlist2 };
            newConfig.Settings.DataSettings.UrlEncodeDataAfterSlicing = legacySettings.EncodeData;
            newConfig.Settings.DataSettings.DataRules = legacySettings.DataRules
                .Select(r => BuildDataRule(r)).ToList();

            // Proxies
            if (legacySettings.OnlySsl)
            {
                newConfig.Settings.ProxySettings.AllowedProxyTypes = new ProxyType[] { ProxyType.Http };
            }
            else if (legacySettings.OnlySocks)
            {
                newConfig.Settings.ProxySettings.AllowedProxyTypes = new ProxyType[] { ProxyType.Socks4, ProxyType.Socks4a, ProxyType.Socks5 };
            }
            newConfig.Settings.ProxySettings.BanLoopEvasion = Math.Max(0, legacySettings.BanLoopEvasionOverride); // -1 needs to be converted to 0
            newConfig.Settings.ProxySettings.MaxUsesPerProxy = legacySettings.MaxProxyUses;
            newConfig.Settings.ProxySettings.UseProxies = legacySettings.NeedsProxies;

            // Inputs
            newConfig.Settings.InputSettings.CustomInputs = legacySettings.CustomInputs
                .Select(i => new CustomInput { VariableName = i.VariableName, Description = i.Description }).ToList();

            // TODO: Selenium settings

            // TODO: Request settings
        }

        private static DataRule BuildDataRule(LegacyDataRule oldRule)
        {
            var supportedSymbols = "@$!%*#?&";
            StringBuilder sb = new('^');

            switch (oldRule.RuleType)
            {
                case LegacyRuleType.MustMatchRegex:
                    return new RegexDataRule
                    {
                        SliceName = oldRule.SliceName,
                        RegexToMatch = oldRule.RuleString
                    };

                case LegacyRuleType.MinLength:
                    return new SimpleDataRule
                    {
                        SliceName = oldRule.SliceName,
                        Comparison = StringRule.ShorterThan,
                        Invert = true,
                        StringToCompare = oldRule.RuleString
                    };

                case LegacyRuleType.MaxLength:
                    return new SimpleDataRule
                    {
                        SliceName = oldRule.SliceName,
                        Comparison = StringRule.LongerThan,
                        Invert = true,
                        StringToCompare = oldRule.RuleString
                    };

                case LegacyRuleType.MustContain:
                    switch (oldRule.RuleString)
                    {
                        case "Lowercase":
                            sb.Append("(?=.*[a-z])");
                            break;

                        case "Uppercase":
                            sb.Append("(?=.*[A-Z])");
                            break;

                        case "Digit":
                            sb.Append("(?=.*\\d)");
                            break;

                        case "Symbol":
                            sb.Append($"(?=.*[{Regex.Escape(supportedSymbols)}])");
                            break;

                        default:
                            sb.Append($"(?=.*{oldRule.RuleString})");
                            break;
                    }

                    return new RegexDataRule
                    {
                        SliceName = oldRule.SliceName,
                        RegexToMatch = sb.ToString()
                    };

                case LegacyRuleType.MustNotContain:
                    var wordToNegate = oldRule.RuleString switch
                    {
                        "Lowercase" => "[a-z]",
                        "Uppercase" => "[A-Z]",
                        "Digit" => "\\d",
                        "Symbol" => $"[{Regex.Escape(supportedSymbols)}]",
                        _ => oldRule.RuleString
                    };
                    sb.Append($"^((?!{wordToNegate}).)*$");

                    return new RegexDataRule
                    {
                        SliceName = oldRule.SliceName,
                        RegexToMatch = sb.ToString()
                    };

                default:
                    throw new NotSupportedException("Unsupported data rule");
            }
        }
    }
}
