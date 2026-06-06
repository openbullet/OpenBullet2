using OpenBullet2.Native.Extensions;
using OpenBullet2.Native.Helpers;
using OpenBullet2.Native.Services;
using OpenBullet2.Native.ViewModels;
using OpenBullet2.Native.Views.Dialogs;
using RuriLib.Models.Configs.Settings;
using RuriLib.Models.Data.Resources.Options;
using RuriLib.Models.Data.Rules;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OpenBullet2.Native.Views.Pages;

/// <summary>
/// Interaction logic for ConfigSettings.xaml
/// </summary>
public partial class ConfigSettings : Page
{
    private readonly IUiFactory uiFactory;
    private readonly ConfigSettingsViewModel vm;

    public ConfigSettings(IUiFactory uiFactory, ConfigSettingsViewModel vm)
    {
        this.uiFactory = uiFactory;
        this.vm = vm;
        DataContext = vm;

        InitializeComponent();
        SetMultiLineTextBoxContents();
    }

    public void UpdateViewModel() => vm.UpdateViewModel();

    private void BlockedUrlsChanged(object sender, TextChangedEventArgs e)
        => vm.BlockedUrls = blockedUrlsTextBox.Text.Split(Environment.NewLine).ToList();

    private void AddCustomInput(object sender, RoutedEventArgs e) => vm.AddCustomInput();
    private void RemoveCustomInput(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: CustomInput input })
        {
            vm.RemoveCustomInput(input);
        }
    }

    private void AddLinesFromFileResource(object sender, RoutedEventArgs e) => vm.AddLinesFromFileResource();
    private void AddRandomLinesFromFileResource(object sender, RoutedEventArgs e) => vm.AddRandomLinesFromFileResource();
    private void RemoveResource(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ConfigResourceOptions resource })
        {
            vm.RemoveResource(resource);
        }
    }

    private void AddSimpleDataRule(object sender, RoutedEventArgs e) => vm.AddSimpleDataRule();
    private void AddRegexDataRule(object sender, RoutedEventArgs e) => vm.AddRegexDataRule();
    private void RemoveDataRule(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: DataRule rule })
        {
            vm.RemoveDataRule(rule);
        }
    }

    private void SetMultiLineTextBoxContents() => blockedUrlsTextBox.Text = string.Join(Environment.NewLine, vm.BlockedUrls);

    private void TestDataRules(object sender, RoutedEventArgs e)
        => new MainDialog(uiFactory.Create<TestDataRulesDialog>(vm.TestDataForRules, vm.TestWordlistTypeForRules, vm.DataRulesCollection), "Test Results").ShowDialog();

    private async void PageKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.S)
        {
            try
            {
                await vm.Save();
                Alert.ToastSuccess("Saved", "The config settings were saved successfully!");
            }
            catch (Exception ex)
            {
                Alert.Exception(ex);
            }
        }
    }
}
