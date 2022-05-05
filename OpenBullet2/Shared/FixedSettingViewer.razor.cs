using Microsoft.AspNetCore.Components;
using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System.Linq;
using System.Threading.Tasks;

namespace OpenBullet2.Shared
{
    public partial class FixedSettingViewer
    {
        [Parameter] public BlockSetting BlockSetting { get; set; }
        [Parameter] public bool DisplayName { get; set; } = true;
        [Parameter] public EventCallback<SettingInputMode> SwitchedMode { get; set; }

        private static bool CouldBeInterpolated(StringSetting x)
            => x.Value.Contains('<') && x.Value.Contains('>');

        private static bool CouldBeInterpolated(ListOfStringsSetting x)
            => x.Value != null && x.Value.Any(v => v.Contains('<') && v.Contains('>'));

        private static bool CouldBeInterpolated(DictionaryOfStringsSetting x)
            => x.Value != null && x.Value.Any(kvp => (kvp.Value.Contains('<') && kvp.Value.Contains('>'))
            || (kvp.Key.Contains('<') && kvp.Key.Contains('>')));

        private async Task SwitchToInterpolated(StringSetting x)
        {
            (BlockSetting.InterpolatedSetting as InterpolatedStringSetting).Value = x.Value;
            await SwitchToInterpolatedMode();
        }

        private async Task SwitchToInterpolated(ListOfStringsSetting x)
        {
            (BlockSetting.InterpolatedSetting as InterpolatedListOfStringsSetting).Value = x.Value;
            await SwitchToInterpolatedMode();
        }

        private async Task SwitchToInterpolated(DictionaryOfStringsSetting x)
        {
            (BlockSetting.InterpolatedSetting as InterpolatedDictionaryOfStringsSetting).Value = x.Value;
            await SwitchToInterpolatedMode();
        }

        private async Task SwitchToInterpolatedMode()
        {
            BlockSetting.InputMode = SettingInputMode.Interpolated;
            await SwitchedMode.InvokeAsync(BlockSetting.InputMode);
        }
    }
}
