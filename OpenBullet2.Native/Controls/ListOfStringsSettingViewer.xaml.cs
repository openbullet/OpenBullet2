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

namespace OpenBullet2.Native.Controls;

/// <summary>
/// Interaction logic for BlockSettingViewer.xaml
/// </summary>
public partial class ListOfStringsSettingViewer : UserControl
{
    private ListOfStringsSettingViewerViewModel? vm;
    private ListOfStringsSettingViewerViewModel ViewModel => vm
        ?? throw new InvalidOperationException("The setting viewer has not been initialized");

    public BlockSetting Setting
    {
        get => ViewModel.Setting;
        set
        {
            if (value.FixedSetting is not ListOfStringsSetting)
            {
                throw new Exception("Invalid setting type for this UC");
            }

            vm = new ListOfStringsSettingViewerViewModel(value);
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

    public ListOfStringsSettingViewer()
    {
        InitializeComponent();
    }

    // Interpolated -> Variable
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
        ViewModel.Value = ViewModel.InterpValue;
        tabControl.SelectedIndex = 1;
        buttonTabControl.SelectedIndex = 1;
    }

    // Constant -> Interpolated
    private void InterpMode(object sender, RoutedEventArgs e)
    {
        ViewModel.Mode = SettingInputMode.Interpolated;
        ViewModel.InterpValue = ViewModel.Value;
        tabControl.SelectedIndex = 2;
        buttonTabControl.SelectedIndex = 2;
    }

    private void SwitchToInterpolatedMode(object sender, MouseButtonEventArgs e)
    {
        ViewModel.Mode = SettingInputMode.Interpolated;
        ViewModel.InterpValue = ViewModel.Value;
        tabControl.SelectedIndex = 2;
        buttonTabControl.SelectedIndex = 2;
    }
}

public class ListOfStringsSettingViewerViewModel : ViewModelBase
{
    public BlockSetting Setting { get; init; }

    public string Name => Setting.ReadableName;

    public string Description => Setting.Description ?? string.Empty;

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
        get => Setting.InputVariableName ?? string.Empty;
        set
        {
            Setting.InputVariableName = value;
            OnPropertyChanged();
        }
    }

    private string interpValue = string.Empty;
    public string InterpValue
    {
        get => interpValue;
        set
        {
            interpValue = value;
            InterpolatedSetting.Value = [.. value.Split(Environment.NewLine, StringSplitOptions.None)];
            OnPropertyChanged();
        }
    }

    private string value = string.Empty;
    public string Value
    {
        get => value;
        set
        {
            this.value = value;
            FixedSetting.Value = [.. value.Split(Environment.NewLine, StringSplitOptions.None)];
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSwitchToInterpolatedMode));
        }
    }

    public ListOfStringsSettingViewerViewModel(BlockSetting setting)
    {
        Setting = setting;

        if (Setting.InputMode == SettingInputMode.Fixed)
        {
            value = FixedSetting.Value is null ? string.Empty : string.Join(Environment.NewLine, FixedSetting.Value);
            interpValue = string.Empty;
        }
        else if (Setting.InputMode == SettingInputMode.Interpolated)
        {
            interpValue = InterpolatedSetting.Value is null ? string.Empty : string.Join(Environment.NewLine, InterpolatedSetting.Value);
            value = string.Empty;
        }
    }

    private ListOfStringsSetting FixedSetting => (ListOfStringsSetting)Setting.FixedSetting!;
    private InterpolatedListOfStringsSetting InterpolatedSetting
        => (InterpolatedListOfStringsSetting)Setting.InterpolatedSetting!;
}
