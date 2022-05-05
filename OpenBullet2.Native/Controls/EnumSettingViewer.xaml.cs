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

        private EnumSetting FixedSetting => Setting.FixedSetting as EnumSetting;

        public string Name => Setting.ReadableName;

        public string Description => Setting.Description;

        public IEnumerable<string> Values => FixedSetting.PrettyNames;

        public string Value
        {
            get => FixedSetting.PrettyName;
            set
            {
                FixedSetting.SetFromPrettyName(value);
                OnPropertyChanged();
            }
        }

        public EnumSettingViewerViewModel(BlockSetting setting)
        {
            Setting = setting;
        }
    }
}
