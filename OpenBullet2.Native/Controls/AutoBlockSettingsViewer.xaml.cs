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
                if (setting.Value.FixedSetting is StringSetting)
                {
                    var viewer = new StringSettingViewer { Setting = setting.Value };
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
