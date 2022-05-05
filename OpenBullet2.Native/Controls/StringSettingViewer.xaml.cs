using OpenBullet2.Native.ViewModels;
using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for BlockSettingViewer.xaml
    /// </summary>
    public partial class StringSettingViewer : UserControl
    {
        private StringSettingViewerViewModel vm;

        public BlockSetting Setting
        {
            get => vm?.Setting;
            set
            {
                if (value.FixedSetting is not StringSetting)
                {
                    throw new Exception("Invalid setting type for this UC");
                }

                vm = new StringSettingViewerViewModel(value);
                DataContext = vm;

                tabControl.SelectedIndex = vm.Mode switch
                {
                    SettingInputMode.Variable => 0,
                    SettingInputMode.Fixed => 1,
                    SettingInputMode.Interpolated => 2,
                    _ => throw new NotImplementedException()
                };

                buttonTabControl.SelectedIndex = vm.Mode switch
                {
                    SettingInputMode.Variable => 0,
                    SettingInputMode.Fixed => 1,
                    SettingInputMode.Interpolated => 2,
                    _ => throw new NotImplementedException()
                };
            }
        }

        public StringSettingViewer()
        {
            InitializeComponent();
        }

        // Interpolated -> Variable
        private void VariableMode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Variable;
            vm.VariableName = vm.InterpValue;
            tabControl.SelectedIndex = 0;
            buttonTabControl.SelectedIndex = 0;
        }

        // Variable -> Constant
        private void ConstantMode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Fixed;
            vm.Value = vm.VariableName;
            tabControl.SelectedIndex = 1;
            buttonTabControl.SelectedIndex = 1;
        }

        // Constant -> Interpolated
        private void InterpMode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Interpolated;
            vm.InterpValue = vm.Value;
            tabControl.SelectedIndex = 2;
            buttonTabControl.SelectedIndex = 2;
        }

        private void SwitchToInterpolatedMode(object sender, MouseButtonEventArgs e)
        {
            vm.Mode = SettingInputMode.Interpolated;
            vm.InterpValue = vm.Value;
            tabControl.SelectedIndex = 2;
            buttonTabControl.SelectedIndex = 2;
        }
    }

    public class StringSettingViewerViewModel : ViewModelBase
    {
        public BlockSetting Setting { get; init; }

        private StringSetting FixedSetting => Setting.FixedSetting as StringSetting;
        private InterpolatedStringSetting InterpolatedSetting => Setting.InterpolatedSetting as InterpolatedStringSetting;

        public string Name => Setting.ReadableName;

        public string Description => Setting.Description;

        public IEnumerable<string> Suggestions => Utils.Suggestions.GetInputVariableSuggestions(Setting);

        public bool CanSwitchToInterpolatedMode => Mode == SettingInputMode.Fixed && Value.Contains('<') && Value.Contains('>');

        public bool MultiLine => FixedSetting.MultiLine;
        public VerticalAlignment VerticalAlignment => MultiLine ? VerticalAlignment.Top : VerticalAlignment.Center;
        public int Height => MultiLine ? 100 : 30;

        public SettingInputMode Mode
        {
            get => Setting.InputMode;
            set
            {
                Setting.InputMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSwitchToInterpolatedMode));
            }
        }

        public string VariableName
        {
            get => Setting.InputVariableName;
            set
            {
                Setting.InputVariableName = value;
                OnPropertyChanged();
            }
        }

        public string InterpValue
        {
            get => (Setting.InterpolatedSetting as InterpolatedStringSetting).Value;
            set
            {
                var s = Setting.InterpolatedSetting as InterpolatedStringSetting;
                s.Value = value;
                OnPropertyChanged();
            }
        }

        public string Value
        {
            get => (Setting.FixedSetting as StringSetting).Value;
            set
            {
                var s = Setting.FixedSetting as StringSetting;
                s.Value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSwitchToInterpolatedMode));
            }
        }

        public StringSettingViewerViewModel(BlockSetting setting)
        {
            Setting = setting;
        }
    }
}
