using RuriLib.Models.Configs;
using System.Collections.Generic;
using System.Linq;

namespace OpenBullet2.Core.Helpers;

/// <summary>
/// Filters custom input answers so only values declared by the config are kept.
/// </summary>
public static class CustomInputAnswerHelper
{
    public static Dictionary<string, string> FilterAnswers(
        Config? config,
        IEnumerable<KeyValuePair<string, string>>? answers)
    {
        if (answers is null)
        {
            return [];
        }

        var filtered = new Dictionary<string, string>();

        if (config is null)
        {
            foreach (var answer in answers)
            {
                filtered[answer.Key] = answer.Value;
            }

            return filtered;
        }

        var validNames = config.Settings.InputSettings.CustomInputs
            .Select(i => i.VariableName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet();

        foreach (var answer in answers)
        {
            if (validNames.Contains(answer.Key))
            {
                filtered[answer.Key] = answer.Value;
            }
        }

        return filtered;
    }
}
