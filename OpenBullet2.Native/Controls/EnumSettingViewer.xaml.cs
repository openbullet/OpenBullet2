using OpenBullet2.Native.ViewModels;
using RuriLib.Extensions;
using RuriLib.Models.Blocks.Settings;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace OpenBullet2.Native.Controls;

/// <summary>
/// Interaction logic for BlockSettingViewer.xaml
/// </summary>
public partial class EnumSettingViewer : UserControl
{
    private EnumSettingViewerViewModel? vm;
    private EnumSettingViewerViewModel ViewModel => vm
        ?? throw new InvalidOperationException("The setting viewer has not been initialized");

    public event EventHandler? ValueChanged;

    public BlockSetting Setting
    {
        get => ViewModel.Setting;
        set
        {
            if (value.FixedSetting is not EnumSetting)
            {
                throw new Exception("Invalid setting type for this UC");
            }

            vm = new EnumSettingViewerViewModel(value);
            vm.ValueChanged += (_, _) => ValueChanged?.Invoke(this, EventArgs.Empty);
            DataContext = vm;
        }
    }

    public EnumSettingViewer()
    {
        InitializeComponent();
    }
}

public class EnumSettingViewerViewModel(BlockSetting setting) : ViewModelBase
{
    public BlockSetting Setting { get; init; } = setting;

    public event EventHandler? ValueChanged;

    private EnumSetting FixedSetting => (EnumSetting)Setting.FixedSetting!;

    public string Name => Setting.ReadableName;

    public string Description => Setting.Description ?? string.Empty;

    public IEnumerable<string> Values => FixedSetting.PrettyNames;

    public string Value
    {
        get => FixedSetting.PrettyName ?? string.Empty;
        set
        {
            FixedSetting.SetFromPrettyName(value);
            OnPropertyChanged();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
