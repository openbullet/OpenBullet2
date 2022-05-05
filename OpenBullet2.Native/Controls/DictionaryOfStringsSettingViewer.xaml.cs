using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.ViewModels;
using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using RuriLib.Models.Blocks.Settings.Interpolated;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for BlockSettingViewer.xaml
    /// </summary>
    public partial class DictionaryOfStringsSettingViewer : UserControl
    {
        private DictionaryOfStringsSettingViewerViewModel vm;

        public BlockSetting Setting
        {
            get => vm?.Setting;
            set
            {
                if (value.FixedSetting is not DictionaryOfStringsSetting)
                {
                    throw new Exception("Invalid setting type for this UC");
                }

                vm = new DictionaryOfStringsSettingViewerViewModel(value);
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

        public DictionaryOfStringsSettingViewer()
        {
            InitializeComponent();
        }

        // Interpolated -> Variable
        private void VariableMode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Variable;
            tabControl.SelectedIndex = 0;
            buttonTabControl.SelectedIndex = 0;
        }

        // Variable -> Constant
        private void ConstantMode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Fixed;
            vm.Value = vm.InterpValue;
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

    public class DictionaryOfStringsSettingViewerViewModel : ViewModelBase
    {
        public BlockSetting Setting { get; init; }

        public string Name => Setting.ReadableName;

        public string Description => Setting.Description;

        public IEnumerable<string> Suggestions => Utils.Suggestions.GetInputVariableSuggestions(Setting);

        public bool CanSwitchToInterpolatedMode => Mode == SettingInputMode.Fixed && Value.Contains('<') && Value.Contains('>');

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

        private string interpValue;
        public string InterpValue
        {
            get => interpValue;
            set
            {
                interpValue = value;
                var s = Setting.InterpolatedSetting as InterpolatedDictionaryOfStringsSetting;
                s.Value = MakeDictionary(value);
                OnPropertyChanged();
            }
        }

        private string value;
        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                var s = Setting.FixedSetting as DictionaryOfStringsSetting;
                s.Value = MakeDictionary(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSwitchToInterpolatedMode));
            }
        }

        public DictionaryOfStringsSettingViewerViewModel(BlockSetting setting)
        {
            Setting = setting;

            if (Setting.InputMode == SettingInputMode.Fixed)
            {
                var s = Setting.FixedSetting as DictionaryOfStringsSetting;
                value = MakeString(s.Value);
                interpValue = string.Empty;
            }
            else if (Setting.InputMode == SettingInputMode.Interpolated)
            {
                var s = Setting.InterpolatedSetting as InterpolatedDictionaryOfStringsSetting;
                interpValue = MakeString(s.Value);
                value = string.Empty;
            }
        }

        private static Dictionary<string, string> MakeDictionary(string value)
            => value?.Split(Environment.NewLine, StringSplitOptions.None)
                    .Where(v => v.Contains(':'))
                    .Select(v => v.Split(':', 2))
                    .DistinctBy(v => v[0])
                    .ToDictionary(v => v[0], v => v[1].StartsWith(' ') ? v[1][1..] : v[1]);

        private static string MakeString(Dictionary<string, string> dict)
            => dict is null
                ? string.Empty
                : string.Join(Environment.NewLine, dict.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
    }
}
