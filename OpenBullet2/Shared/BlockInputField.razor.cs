using Microsoft.AspNetCore.Components;
using OpenBullet2.Core.Services;
using OpenBullet2.Services;
using Radzen;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Custom;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared
{
    public partial class BlockInputField
    {
        [Inject] private RuriLibSettingsService RuriLibSettings { get; set; }
        [Inject] private VolatileSettingsService VolatileSettings { get; set; }
        [Inject] private ConfigService ConfigService { get; set; }

        [Parameter] public BlockSetting BlockSetting { get; set; }
        [Parameter] public bool DisplayName { get; set; } = true;

        private ElementReference settingNameLabel;

        private Task<IEnumerable<string>> GetSuggestions(string partial)
        {
            var suggestions = new List<string> {
            "data.SOURCE", "data.ERROR", "data.ADDRESS",
            "data.HEADERS[\"name\"]", "data.COOKIES[\"name\"]",
            "data.STATUS", "data.RESPONSECODE", "data.RAWSOURCE", "data.Line.Data" };

            var wordlistTypeName = VolatileSettings.DebuggerOptions.WordlistType;
            var wordlistType = RuriLibSettings.Environment.WordlistTypes.First(w => w.Name == wordlistTypeName);
            foreach (var slice in wordlistType.Slices.Concat(wordlistType.SlicesAlias).Reverse())
            {
                suggestions.Insert(0, $"input.{slice}");
            }

            var stack = ConfigService.SelectedConfig.Stack;

            foreach (var block in stack)
            {
                // If it's the current block, stop here (we don't want to add variables from this or the next blocks)
                if (block.Settings.Any(s => s.Value == BlockSetting))
                {
                    break;
                }

                foreach (var variable in GetOutputVariables(block).Reverse())
                {
                    if (!string.IsNullOrWhiteSpace(variable) && !suggestions.Contains(variable))
                    {
                        suggestions.Insert(0, variable);
                    }
                }
            }

            var filtered = string.IsNullOrWhiteSpace(partial)
                ? suggestions
                : suggestions.Where(s => s.StartsWith(partial, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(filtered);
        }

        private IEnumerable<string> GetOutputVariables(BlockInstance block)
            => block switch
            {
                AutoBlockInstance x => x.Descriptor.ReturnType == null ? Array.Empty<string>() : new string[] { x.OutputVariable },
                ParseBlockInstance x => new string[] { x.OutputVariable },
                ScriptBlockInstance x => x.OutputVariables.Select(v => v.Name),
                _ => Array.Empty<string>()
            };
    }
}
