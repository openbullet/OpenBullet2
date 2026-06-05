using OpenBullet2.Native.ViewModels;
using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls;

/// <summary>
/// Interaction logic for BlockSettingViewer.xaml
/// </summary>
public partial class FloatSettingViewer : UserControl
{
    private FloatSettingViewerViewModel? vm;
    private FloatSettingViewerViewModel ViewModel => vm
        ?? throw new InvalidOperationException("The setting viewer has not been initialized");

    public BlockSetting Setting
    {
        get => ViewModel.Setting;
        set
        {
            if (value.FixedSetting is not FloatSetting)
            {
                throw new Exception("Invalid setting type for this UC");
            }

            vm = new FloatSettingViewerViewModel(value);
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

    public FloatSettingViewer()
    {
        InitializeComponent();
    }

    // Constant -> Variable
    private void VariableMode(object sender, RoutedEventArgs e)
    {
        ViewModel.Mode = SettingInputMode.Variable;
        tabControl.SelectedIndex = 0;
        buttonTabControl.SelectedIndex = 0;
    }

    // Variable -> Constant
    private void ConstantMode(object sender, RoutedEventArgs e)
    {
        ViewModel.Mode = SettingInputMode.Fixed;
        tabControl.SelectedIndex = 1;
        buttonTabControl.SelectedIndex = 1;
    }
}

public class FloatSettingViewerViewModel(BlockSetting setting) : ViewModelBase
{
    public BlockSetting Setting { get; init; } = setting;

    public string Name => Setting.ReadableName;

    public string Description => Setting.Description ?? string.Empty;

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
        get => Setting.InputVariableName ?? string.Empty;
        set
        {
            Setting.InputVariableName = value;
            OnPropertyChanged();
        }
    }

    public double Value
    {
        get => FixedSetting.Value;
        set
        {
            FixedSetting.Value = value;
            OnPropertyChanged();
        }
    }

    private FloatSetting FixedSetting => (FloatSetting)Setting.FixedSetting!;
}
