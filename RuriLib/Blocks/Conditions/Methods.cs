using RuriLib.Attributes;
using RuriLib.Extensions;
using RuriLib.Models.Bots;
using RuriLib.Models.Conditions.Comparisons;
using System.Collections.Generic;

namespace RuriLib.Blocks.Conditions;

/// <summary>
/// Blocks to check conditions.
/// </summary>
[BlockCategory("Conditions", "Blocks that have to do with checking conditions", "#1e90ff")]
public static class Methods
{
    /*
     * These are not blocks, but they take BotData as an input. The KeycheckBlockInstance will take care
     * of writing C# code that calls these methods where necessary once it's transpiled.
     */

    /// <summary>
    /// Checks a condition with two boolean terms.
    /// </summary>
    public static bool CheckCondition(BotData data, bool leftTerm, BoolComparison comparison, bool rightTerm)
    {
        data.Logger.LogHeader();
        
        var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);
            
        if (result)
        {
            data.Logger.Log($"Matched key {leftTerm} {comparison} {rightTerm}");
        }

        return result;
    }

    /// <summary>
    /// Checks a condition with two string terms.
    /// </summary>
    public static bool CheckCondition(BotData data, string leftTerm, StrComparison comparison, string rightTerm)
    {
        data.Logger.LogHeader();
        
        var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

        if (result)
        {
            data.Logger.Log($"Matched key {leftTerm.TruncatePretty(50)} {comparison} {rightTerm.TruncatePretty(50)}");
        }

        return result;
    }

    /// <summary>
    /// Checks a condition between a list of strings and a string.
    /// </summary>
    public static bool CheckCondition(BotData data, List<string> leftTerm, ListComparison comparison, string rightTerm)
    {
        data.Logger.LogHeader();
        
        var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

        if (result)
        {
            data.Logger.Log($"Matched key {leftTerm.AsString().TruncatePretty(50)} {comparison} {rightTerm.TruncatePretty(50)}");
        }

        return result;
    }

    /// <summary>
    /// Checks a condition with two integer terms.
    /// </summary>
    public static bool CheckCondition(BotData data, int leftTerm, NumComparison comparison, int rightTerm)
    {
        data.Logger.LogHeader();
        
        var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

        if (result)
        {
            data.Logger.Log($"Matched key {leftTerm} {comparison} {rightTerm}");
        }

        return result;
    }

    /// <summary>
    /// Checks a condition with two float terms.
    /// </summary>
    public static bool CheckCondition(BotData data, float leftTerm, NumComparison comparison, float rightTerm)
    {
        data.Logger.LogHeader();
        
        var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

        if (result)
        {
            data.Logger.Log($"Matched key {leftTerm} {comparison} {rightTerm}");
        }

        return result;
    }

    /// <summary>
    /// Checks a condition between a dictionary of strings and a string.
    /// </summary>
    public static bool CheckCondition(BotData data, Dictionary<string, string> leftTerm, DictComparison comparison, string rightTerm)
    {
        data.Logger.LogHeader();
        
        var result = RuriLib.Functions.Conditions.Conditions.Check(leftTerm, comparison, rightTerm);

        if (result)
        {
            data.Logger.Log($"Matched key {leftTerm.AsString().TruncatePretty(50)} {comparison} {rightTerm.TruncatePretty(50)}");
        }

        return result;
    }

    /// <summary>
    /// Checks if the source contains a global ban key.
    /// </summary>
    public static bool CheckGlobalBanKeys(BotData data)
    {
        data.Logger.LogHeader();
        
        var result = data.Providers.ProxySettings.ContainsBanKey(data.SOURCE, out var matchedKey);
            
        if (result)
        {
            data.Logger.Log($"Found global ban key: {matchedKey}");
        }

        return result;
    }

    /// <summary>
    /// Checks if the source contains a global retry key.
    /// </summary>
    public static bool CheckGlobalRetryKeys(BotData data)
    {
        data.Logger.LogHeader();
        
        var result = data.Providers.ProxySettings.ContainsRetryKey(data.SOURCE, out var matchedKey);

        if (result)
        {
            data.Logger.Log($"Found global retry key: {matchedKey}");
        }

        return result;
    }
}
