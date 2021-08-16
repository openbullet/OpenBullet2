using OpenBullet2.Native.ViewModels;
using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for BlockSettingViewer.xaml
    /// </summary>
    public partial class EnumSettingViewer : UserControl
    {
        private EnumSettingViewerViewModel vm;

        public BlockSetting Setting
        {
            get => vm?.Setting;
            set
            {
                if (value.FixedSetting is not EnumSetting)
                {
                    throw new Exception("Invalid setting type for this UC");
                }

                vm = new EnumSettingViewerViewModel(value);
                DataContext = vm;
            }
        }

        public EnumSettingViewer()
        {
            InitializeComponent();
        }
    }

    public class EnumSettingViewerViewModel : ViewModelBase
    {
        public BlockSetting Setting { get; init; }

        public string Name => Setting.Name.ToReadableName();

        public IEnumerable<string> Values => Enum.GetNames((Setting.FixedSetting as EnumSetting).EnumType);

        public string Value
        {
            get => (Setting.FixedSetting as EnumSetting).Value;
            set
            {
                var s = Setting.FixedSetting as EnumSetting;
                s.Value = value;
                OnPropertyChanged();
            }
        }

        public EnumSettingViewerViewModel(BlockSetting setting)
        {
            Setting = setting;
        }
    }
}
