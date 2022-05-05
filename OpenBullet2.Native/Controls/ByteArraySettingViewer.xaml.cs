using OpenBullet2.Native.ViewModels;
using RuriLib.Extensions;
using RuriLib.Functions.Conversion;
using RuriLib.Models.Blocks.Settings;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls
{
    /// <summary>
    /// Interaction logic for BlockSettingViewer.xaml
    /// </summary>
    public partial class ByteArraySettingViewer : UserControl
    {
        private ByteArraySettingViewerViewModel vm;

        public BlockSetting Setting
        {
            get => vm?.Setting;
            set
            {
                if (value.FixedSetting is not ByteArraySetting)
                {
                    throw new Exception("Invalid setting type for this UC");
                }

                vm = new ByteArraySettingViewerViewModel(value);
                DataContext = vm;

                tabControl.SelectedIndex = vm.Mode switch
                {
                    SettingInputMode.Variable => 0,
                    SettingInputMode.Fixed => 1,
                    _ => throw new NotImplementedException()
                };

                buttonTabControl.SelectedIndex = vm.Mode switch
                {
                    SettingInputMode.Variable => 0,
                    SettingInputMode.Fixed => 1,
                    _ => throw new NotImplementedException()
                };
            }
        }

        public ByteArraySettingViewer()
        {
            InitializeComponent();
        }

        // Constant Hex -> Variable
        private void VariableMode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Variable;
            tabControl.SelectedIndex = 0;
            buttonTabControl.SelectedIndex = 0;
        }

        // Variable -> Constant B64
        private void B64Mode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Fixed;
            tabControl.SelectedIndex = 1;
            buttonTabControl.SelectedIndex = 1;
        }

        private void HexMode(object sender, RoutedEventArgs e)
        {
            vm.Mode = SettingInputMode.Fixed;
            tabControl.SelectedIndex = 2;
            buttonTabControl.SelectedIndex = 2;
        }
    }

    public class ByteArraySettingViewerViewModel : ViewModelBase
    {
        public BlockSetting Setting { get; init; }

        public string Name => Setting.ReadableName;

        public string Description => Setting.Description;

        public IEnumerable<string> Suggestions => Utils.Suggestions.GetInputVariableSuggestions(Setting);

        public SettingInputMode Mode
        {
            get => Setting.InputMode;
            set
            {
                Setting.InputMode = value;
                OnPropertyChanged();
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

        private string b64Value;
        public string B64Value
        {
            get => b64Value;
            set
            {
                b64Value = value;
                var s = Setting.FixedSetting as ByteArraySetting;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        s.Value = Base64Converter.ToByteArray(value);
                        hexValue = HexConverter.ToHexString(s.Value);
                    }
                    catch
                    {
                        
                    }
                }
                else
                {
                    s.Value = null;
                }
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(HexValue));
            }
        }

        private string hexValue;
        public string HexValue
        {
            get => hexValue;
            set
            {
                hexValue = value;
                var s = Setting.FixedSetting as ByteArraySetting;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    try
                    {
                        s.Value = HexConverter.ToByteArray(value, false);
                        b64Value = Base64Converter.ToBase64String(s.Value);
                    }
                    catch
                    {
                        
                    }
                }
                else
                {
                    s.Value = null;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(B64Value));
            }
        }

        public ByteArraySettingViewerViewModel(BlockSetting setting)
        {
            Setting = setting;

            var s = Setting.FixedSetting as ByteArraySetting;
            b64Value = s.Value is null ? string.Empty : Base64Converter.ToBase64String(s.Value);
            hexValue = s.Value is null ? string.Empty : HexConverter.ToHexString(s.Value);
        }
    }
}
