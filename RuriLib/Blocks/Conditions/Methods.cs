using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Models.Bots;
using RuriLib.Models.Conditions.Comparisons;
using System.Collections.Generic;

namespace RuriLib.Blocks.Conditions
{
    [BlockCategory("Conditions", "Blocks that have to do with checking conditions", "#1e90ff")]
    public static class Methods
    {
        /*
         * These are not blocks, but they take BotData as an input. The KeycheckBlockInstance will take care
         * of writing C# code that calls these methods where necessary once it's transpiled.
         */

        public static bool CheckCondition(BotData data, bool leftTerm, BoolComparison comparison, bool rightTerm)
        {
            var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);
            
            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Matched key {leftTerm} {comparison} {rightTerm}");
            }

            return result;
        }

        public static bool CheckCondition(BotData data, string leftTerm, StrComparison comparison, string rightTerm)
        {
            var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Matched key {leftTerm.TruncatePretty(50)} {comparison} {rightTerm.TruncatePretty(50)}");
            }

            return result;
        }

        public static bool CheckCondition(BotData data, List<string> leftTerm, ListComparison comparison, string rightTerm)
        {
            var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Matched key {leftTerm.AsString().TruncatePretty(50)} {comparison} {rightTerm.TruncatePretty(50)}");
            }

            return result;
        }

        public static bool CheckCondition(BotData data, int leftTerm, NumComparison comparison, int rightTerm)
        {
            var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Matched key {leftTerm} {comparison} {rightTerm}");
            }

            return result;
        }

        public static bool CheckCondition(BotData data, float leftTerm, NumComparison comparison, float rightTerm)
        {
            var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Matched key {leftTerm} {comparison} {rightTerm}");
            }

            return result;
        }

        public static bool CheckCondition(BotData data, Dictionary<string, string> leftTerm, DictComparison comparison, string rightTerm)
        {
            var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Matched key {leftTerm.AsString().TruncatePretty(50)} {comparison} {rightTerm.TruncatePretty(50)}");
            }

            return result;
        }

        public static bool CheckGlobalBanKeys(BotData data)
        {
            var result = data.Providers.ProxySettings.ContainsBanKey(data.SOURCE, out var matchedKey);
            
            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Found global ban key: {matchedKey}");
            }

            return result;
        }

        public static bool CheckGlobalRetryKeys(BotData data)
        {
            var result = data.Providers.ProxySettings.ContainsRetryKey(data.SOURCE, out var matchedKey);

            if (result)
            {
                data.Logger.LogHeader();
                data.Logger.Log($"Found global retry key: {matchedKey}");
            }

            return result;
        }
    }
}
