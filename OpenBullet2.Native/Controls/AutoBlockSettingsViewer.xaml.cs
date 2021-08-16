using OpenBullet2.Native.ViewModels;
using RuriLib.Models.Blocks;
using RuriLib.Models.Blocks.Settings;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for AutoBlockSettingsViewer.xaml
    /// </summary>
    public partial class AutoBlockSettingsViewer : UserControl
    {
        private readonly AutoBlockSettingsViewerViewModel vm;

        public AutoBlockSettingsViewer(AutoBlockInstance block)
        {
            vm = new AutoBlockSettingsViewerViewModel(block);
            DataContext = vm;

            InitializeComponent();
            CreateControls();
        }

        private void CreateControls()
        {
            foreach (var setting in vm.Block.Settings)
            {
                UserControl viewer = setting.Value.FixedSetting switch
                {
                    StringSetting => new StringSettingViewer { Setting = setting.Value },
                    IntSetting => new IntSettingViewer { Setting = setting.Value },
                    FloatSetting => new FloatSettingViewer { Setting = setting.Value },
                    EnumSetting => new EnumSettingViewer { Setting = setting.Value },
                    ByteArraySetting => new ByteArraySettingViewer { Setting = setting.Value },
                    BoolSetting => new BoolSettingViewer { Setting = setting.Value },
                    _ => null
                };

                if (viewer is not null)
                {
                    settingsPanel.Children.Add(viewer);
                }
            }
        }
    }

    public class AutoBlockSettingsViewerViewModel : ViewModelBase
    {
        public AutoBlockInstance Block { get; init; }

        public AutoBlockSettingsViewerViewModel(AutoBlockInstance block)
        {
            Block = block;
        }
    }
}
