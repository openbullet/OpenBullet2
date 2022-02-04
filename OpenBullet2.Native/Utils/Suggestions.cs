using OpenBullet2.Core.Services;
using OpenBullet2.Native.Services;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenBullet2.Native.Utils
{
    public static class Suggestions
    {
        public static IEnumerable<string> GetInputVariableSuggestions()
        {
            var debuggerVM = SP.GetService<ViewModelsService>().Debugger;
            var rlSettings = SP.GetService<RuriLibSettingsService>();
            var configService = SP.GetService<ConfigService>();

            var suggestions = new List<string> {
            "data.SOURCE", "data.ERROR", "data.ADDRESS",
            "data.HEADERS[\"name\"]", "data.COOKIES[\"name\"]",
            "data.STATUS", "data.RESPONSECODE", "data.RAWSOURCE", "data.Line.Data" };

            var wordlistTypeName = debuggerVM.WordlistType;
            var wordlistType = rlSettings.Environment.WordlistTypes.First(w => w.Name == wordlistTypeName);
            foreach (var slice in wordlistType.Slices.Concat(wordlistType.SlicesAlias).Reverse())
            {
                suggestions.Insert(0, $"input.{slice}");
            }

            // TODO: Ideally only show variables in blocks above this one
            var stack = configService.SelectedConfig.Stack;
            foreach (var variable in stack.Select(b => GetOutputVariables(b)).SelectMany(v => v).Reverse())
            {
                if (!string.IsNullOrWhiteSpace(variable) && !suggestions.Contains(variable))
                {
                    suggestions.Insert(0, variable);
                }
            }

            return suggestions;
        }

        private static IEnumerable<string> GetOutputVariables(BlockInstance block)
            => block switch
            {
                AutoBlockInstance x => x.Descriptor.ReturnType == null ? Array.Empty<string>() : new string[] { x.OutputVariable },
                ParseBlockInstance x => new string[] { x.OutputVariable },
                ScriptBlockInstance x => x.OutputVariables.Select(v => v.Name),
                _ => Array.Empty<string>()
            };
    }
}
